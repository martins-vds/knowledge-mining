param docsContainerName string = 'documents'
param synonymsContainerName string = 'synonyms'
param deployFunction bool = true
param location string = resourceGroup().location
param servicePrincipalId string = ''

var uniqueness = uniqueString(resourceGroup().id)
var keyVaultName = 'akv-${uniqueness}'
var searchName = 'search-${uniqueness}'
var cognitiveAccountName = 'cognitive-account-${uniqueness}'
var signalRAccountName = 'signalr-${uniqueness}'
var storageAccountNameData = 'stg${uniqueness}'
var appServicePlanName = 'app-plan-${uniqueness}'
var webAppName = 'site-${uniqueness}'
var functionAppName = 'function-app-${uniqueness}'
var appInsightsName = 'app-insights-${uniqueness}'
var appInsightsWorkspaceName = 'workspace-${uniqueness}'

var secretKeySearch = 'SEARCHSERVICESECRET'
var secretKeySignalR = 'SIGNALRCONNECTIONSTRING'
var secretKeyCognitive = 'COGNITIVESERVICESSECRET'
var secretKeyStorageKey = 'STORAGEACCOUNTKEYSECRET'
var secretKeyStorageConnectionString = 'STORAGEACCOUNTCONNECTIONSTRING'

var subnetAppServiceName = 'AppService'
var subnetPrivateEndpointsName = 'PrivateEndpoints'
var privateDnsZone = 'privatelink.blob.core.windows.net'

var blobDataContributorRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var blobDataReaderRoleDefinitionId = resourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')

/*
  Example:

  var ipAddressToAllow = [
    {
      value: 'x.x.x.x'
      action: 'Allow'
    }
    {
      value: 'y.y.y.y/a'
      action: 'Allow'
    }
  ]
*/
var ipAddressToAllow = []

// Networking
resource vnet 'Microsoft.Network/virtualNetworks@2020-06-01' = {
  location: location
  name: 'vnet'
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetAppServiceName
        properties: {
          addressPrefix: '10.0.1.0/24'
          delegations: [
            {
              name: 'appservice-serverfarm'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: subnetPrivateEndpointsName
        properties: {
          addressPrefix: '10.0.2.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

resource storageBlobPrivateZone 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZone
  location: 'global'
}

resource storageBlobPrivateZoneVirtualNetworkLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  name: '${storageBlobPrivateZone.name}/${uniqueString(vnet.id)}'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnet.id
    }
    registrationEnabled: false
  }
}

// Key Vault
resource azure_key_vault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'premium'
    }
    tenantId: subscription().tenantId
    enableSoftDelete: true
    enablePurgeProtection: true
    softDeleteRetentionInDays: 7
    enableRbacAuthorization: false
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    accessPolicies: empty(servicePrincipalId) ? [
      {
        objectId: app_services_website.identity.principalId
        tenantId: app_services_website.identity.tenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ] : [
      {
        objectId: servicePrincipalId
        tenantId: subscription().tenantId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
          ]
        }
      }
      {
        objectId: app_services_website.identity.principalId
        tenantId: app_services_website.identity.tenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ]
  }
}

resource akv_secret_storage_account_resource_id 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/STORAGEACCOUNTRESOURCEID'
  properties: {
    value: azure_storage_account_data.id
  }
}

resource akv_secret_storage_account_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/${secretKeyStorageConnectionString}'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${azure_storage_account_data.name};AccountKey=${listKeys(azure_storage_account_data.id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
  }
}

resource akv_secret_storage_account_key_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/${secretKeyStorageKey}'
  properties: {
    value: listKeys(azure_storage_account_data.id, '2019-06-01').keys[0].value
  }
}

resource akv_secret_search_endpoint 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/SEARCHSERVICEENDPOINT'
  properties: {
    value: 'https://${azure_search_service.name}.search.windows.net'
  }
}

resource akv_secret_search_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/${secretKeySearch}'
  properties: {
    value: listAdminKeys(azure_search_service.id, '2020-08-01').primaryKey
  }
}

resource akv_secret_cognitive_services_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/${secretKeyCognitive}'
  properties: {
    value: listKeys(azure_congnitive_account.id, '2017-04-18').key1
  }
}

resource akv_secret_signalr_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/${secretKeySignalR}'
  properties:{
    value: azure_signal_r.listKeys().primaryConnectionString
  }
}

// Search
resource azure_search_service 'Microsoft.Search/searchServices@2020-08-01' = {
  name: searchName
  location: location
  sku: {
    name: 'standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Cognitive Services
resource azure_congnitive_account 'Microsoft.CognitiveServices/accounts@2017-04-18' = {
  name: cognitiveAccountName
  location: location
  kind: 'CognitiveServices'
  sku: {
    name: 'S0'
  }
}

resource azure_signal_r 'Microsoft.SignalRService/signalR@2022-02-01' = {
  name: signalRAccountName
  location: location
  sku:{
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 1
  }
  properties:{
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    features: []
    publicNetworkAccess: 'Enabled'
  }
}

// Storage Accounts
resource azure_storage_account_data 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: storageAccountNameData
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'AzureServices, Logging, Metrics'
      ipRules: ipAddressToAllow
    }
  }
}

resource azure_storage_account_blob_retention 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  name: '${azure_storage_account_data.name}/default'
  properties: {
    cors:{
      corsRules: []
    }
    deleteRetentionPolicy:{
      allowPermanentDelete: false
      enabled: true
      days: 7
    }
  }
}

resource azure_storage_account_container_docs 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
  name: '${azure_storage_account_data.name}/default/${docsContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

resource azure_storage_account_container_syn 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
  name: '${azure_storage_account_data.name}/default/${synonymsContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

resource azure_storage_account_data_blob_pe 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  location: location
  name: '${azure_storage_account_data.name}-blob-endpoint'
  properties: {
    subnet: {
      id: '${vnet.id}/subnets/${subnetPrivateEndpointsName}'
    }
    privateLinkServiceConnections: [
      {
        name: '${azure_storage_account_data.name}-blob-endpoint'
        properties: {
          privateLinkServiceId: azure_storage_account_data.id
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

resource azure_storage_account_data_blob_pe_dns_reg 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-06-01' = {
  name: '${azure_storage_account_data_blob_pe.name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink_blob_core_windows_net'
        properties: {
          privateDnsZoneId: storageBlobPrivateZone.id
        }
      }
    ]
  }
}

resource azure_storage_account_functions 'Microsoft.Storage/storageAccounts@2019-06-01' = if (deployFunction) {
  name: 'stgfunc${uniqueness}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'Logging, Metrics'
    }
  }
}

resource azure_storage_account_functions_blob_pe 'Microsoft.Network/privateEndpoints@2020-06-01' = if (deployFunction) {
  location: location
  name: '${azure_storage_account_functions.name}-blob-endpoint'
  properties: {
    subnet: {
      id: '${vnet.id}/subnets/${subnetPrivateEndpointsName}'
    }
    privateLinkServiceConnections: [
      {
        name: '${azure_storage_account_functions.name}-blob-endpoint'
        properties: {
          privateLinkServiceId: azure_storage_account_functions.id
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

resource azure_storage_account_functions_blob_pe_dns_reg 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-06-01' = if (deployFunction) {
  name: '${azure_storage_account_functions_blob_pe.name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink_blob_core_windows_net'
        properties: {
          privateDnsZoneId: storageBlobPrivateZone.id
        }
      }
    ]
  }
}

// App Service
resource azure_app_service_plan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  kind: 'Linux'
  sku: {
    tier: 'PremiumV3'
    name: 'P1v3'
    family: 'Pv3'
    capacity: 1
    size: 'P1v3'
  }
  properties: {
    reserved: true
  }
}

resource app_insights_workspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: appInsightsWorkspaceName
  location: location
  properties:{
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource app_insights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: app_insights_workspace.id
  }
}

resource app_services_website 'Microsoft.Web/sites@2020-06-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: azure_app_service_plan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      alwaysOn: true
      appSettings: [
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: app_insights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: app_insights.properties.ConnectionString
        }
        {
          name: 'ASPNETCORE_HOSTINGSTARTUPASSEMBLIES'
          value: 'Microsoft.Azure.SignalR'
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'InstrumentationEngine_EXTENSION_VERSION'
          value: 'enabled'
        }
        {
          name: 'SnapshotDebugger_EXTENSION_VERSION'
          value: 'enabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_BaseExtensions'
          value: 'enabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_PreemptSdk'
          value: 'enabled'
        }
        {
          name: 'WEBSITE_DNS_SERVER' // required for VNET Integration + Azure DNS Private Zones
          value: '168.63.129.16'
        }
        {
          name: 'WEBSITE_VNET_ROUTE_ALL'
          value: '1'
        }
        {
          name: 'AzureMaps__SubscriptionKey'
          value: ''
        }
        {
          name: 'Search__Endpoint'
          value: 'https://${azure_search_service.name}.search.windows.net'
        }
        {
          name: 'Search__Credential__Key'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeySearch})'
        }
        {
          name: 'Search__IndexName'
          value: 'km'
        }
        {
          name: 'Search__IndexerName'
          value: 'km-indexer'
        }
        {
          name: 'Search__KeyField'
          value: 'metadata_storage_path'
        }
        {
          name: 'Search__SuggesterName'
          value: 'sg'
        }
        {
          name: 'Search__PageSize'
          value: '10'
        }
        {
          name: 'Search__IsPathBase64Encoded'
          value: 'true'
        }
        {
          name: 'Azure__SignalR__Enabled'
          value: 'true'
        }
        {
          name: 'Azure__SignalR__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeySignalR})'
        }
        {
          name: 'Storage__ServiceUri'
          value: azure_storage_account_data.properties.primaryEndpoints.blob
        }
        {
          name: 'Storage__AccountKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeyStorageKey})'
        }
        {
          name: 'Storage__ContainerName'
          value: docsContainerName
        }
        {
          name: 'EntityMap__Facets'
          value: 'keyPhrases, locations'
        }
        {
          name: 'Customizations__Enabled'
          value: 'true'
        }
        {
          name: 'Customizations__OrganizationName'
          value: 'Microsoft'
        }
        {
          name: 'Customizations__OrganizationLogo'
          value: '~/images/logo.png'
        }
        {
          name: 'Customizations__OrganizationWebSiteUrl'
          value: 'https://www.microsoft.com'
        }
      ]
    }
    httpsOnly: true
    clientAffinityEnabled: false
  }
}

resource app_services_website_vnet 'Microsoft.Web/sites/networkConfig@2020-06-01' = {
  name: '${app_services_website.name}/VirtualNetwork'
  properties: {
    subnetResourceId: '${vnet.id}/subnets/${subnetAppServiceName}'
    swiftSupported: true
  }
}

resource app_services_function_app 'Microsoft.Web/sites@2020-06-01' = if (deployFunction) {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: azure_app_service_plan.id
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'DOTNET|6.0'
      appSettings: [
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: app_insights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: app_insights.properties.ConnectionString
        }        
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'InstrumentationEngine_EXTENSION_VERSION'
          value: 'enabled'
        }
        {
          name: 'SnapshotDebugger_EXTENSION_VERSION'
          value: 'enabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_BaseExtensions'
          value: 'enabled'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_PreemptSdk'
          value: 'enabled'
        }
        {
          name: 'WEBSITE_DNS_SERVER' // required for VNET Integration + Azure DNS Private Zones
          value: '168.63.129.16'
        }
        {
          name: 'WEBSITE_VNET_ROUTE_ALL'
          value: '1'
        }        
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '10.14.1'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AzureWebJobsStorage'
          value: deployFunction ? 'DefaultEndpointsProtocol=https;AccountName=${azure_storage_account_functions.name};AccountKey=${listKeys(azure_storage_account_functions.id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net' : ''
        }
        {
          name: 'SynonymsStorage'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeyStorageConnectionString})'
        }
      ]
    }
    httpsOnly: true
  }
}

resource akv_secret_function_app_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${azure_key_vault.name}/FUNCTIONADMINKEY'
  properties: {
    value: listKeys('${app_services_function_app.id}/host/default', '2021-02-01').functionKeys.default
  }
}

resource function_access_policy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = if (deployFunction) {
  name: 'add'
  parent: azure_key_vault
  properties: {
    accessPolicies: deployFunction ? [
      {
        objectId: app_services_function_app.identity.principalId
        tenantId: app_services_function_app.identity.tenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ] : []
  }
}

resource app_services_function_app_vnet 'Microsoft.Web/sites/networkConfig@2020-06-01' = if (deployFunction) {
  name: '${app_services_function_app.name}/VirtualNetwork'
  properties: {
    subnetResourceId: '${vnet.id}/subnets/${subnetAppServiceName}'
    swiftSupported: true
  }
}

// Role Assignments
resource roleAssignSearchToStorageBlobReader 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(azure_storage_account_data.id, blobDataReaderRoleDefinitionId, 'Blob Data Reader')
  scope: azure_storage_account_data
  properties: {
    roleDefinitionId: blobDataReaderRoleDefinitionId
    principalId: azure_search_service.identity.principalId
  }
}

resource roleAssignSiteToStorageBlobContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(azure_storage_account_data.id, blobDataContributorRoleDefinitionId, 'Blob Data Contributor')
  scope: azure_storage_account_data
  properties: {
    roleDefinitionId: blobDataContributorRoleDefinitionId
    principalId: app_services_website.identity.principalId
  }
}

output storage_data_id string = azure_storage_account_data.id
output storage_data_name string = azure_storage_account_data.name
output search_endpoint string = 'https://${azure_search_service.name}.search.windows.net'
output keyvault_name string = azure_key_vault.name
output app_name string = app_services_website.name
output skills_name string = app_services_function_app.name
