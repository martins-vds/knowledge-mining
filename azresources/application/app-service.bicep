// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param appServicePlanName string
param webAppName string
param azure_search_service_name string
param keyVaultName string
param secretKeySearch string
param azure_storage_account_data_name string
param docsContainerName string
param vnetID string
param subnetAppServiceName string
param secretKeyStorageKey string
param app_insightsInstrumentationKey string


// App Service
resource azure_app_service_plan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: resourceGroup().location
  kind: 'Linux'
  sku: {
    tier: 'Standard'
    name: 'S1'
  }
  properties: {
    reserved: true
  }
}


resource app_services_website 'Microsoft.Web/sites@2020-06-01' = {
  name: webAppName
  location: resourceGroup().location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: azure_app_service_plan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|3.1'
      appSettings: [
        {
          name: 'WEBSITE_DNS_SERVER' // required for VNET Integration + Azure DNS Private Zones
          value: '168.63.129.16'
        }
        {
          name: 'WEBSITE_VNET_ROUTE_ALL'
          value: '1'
        }
        {
          name: 'SearchServiceName'
          value: azure_search_service_name
        }
        {
          name: 'SearchApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeySearch})'
        }
        {
          name: 'SearchIndexName'
          value: 'km'
        }
        {
          name: 'SearchIndexerName'
          value: 'km-indexer'
        }
        {
          name: 'StorageAccountName'
          value: azure_storage_account_data_name
        }
        {
          name: 'StorageAccountKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${secretKeyStorageKey})'
        }
        {
          name: 'StorageContainerAddress'
          value: 'https://${azure_storage_account_data_name}.blob.core.windows.net/${docsContainerName}'
        }
        {
          name: 'KeyField'
          value: 'metadata_storage_path'
        }
        {
          name: 'IsPathBase64Encoded'
          value: 'true'
        }
        {
          name: 'InstrumentationKey'
          value: app_insightsInstrumentationKey
        }
      ]
    }
  }
}

resource app_services_website_vnet 'Microsoft.Web/sites/networkConfig@2020-06-01' = {
  name: '${app_services_website.name}/VirtualNetwork'
  properties: {
    subnetResourceId: '${vnetID}/subnets/${subnetAppServiceName}'
    swiftSupported: true
  } 
}

output app_service_plan_id string = azure_app_service_plan.id
output app_services_website_principalId string = app_services_website.identity.principalId
output app_services_website_tenantId string = app_services_website.identity.tenantId
