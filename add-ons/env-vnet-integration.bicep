// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param docsContainerName string = 'documents'
param spnObjectId string

param vnetId string
param storageBlobPrivateZoneId string

param subnetAppServiceName string
param subnetPrivateEndpointsName string

var keyVaultName = 'akv-${uniqueString(resourceGroup().id)}'
var searchName = 'search-${uniqueString(resourceGroup().id)}'
var cognitiveAccountName = 'cognitive-account-${uniqueString(resourceGroup().id)}'
var storageAccountNameData = 'stg${uniqueString(resourceGroup().id)}'
var appServicePlanName = 'app-plan-${uniqueString(resourceGroup().id)}'
var webAppName = 'site-${uniqueString(resourceGroup().id)}'
var functionAppName = 'function-app-${uniqueString(resourceGroup().id)}'
var appInsightsName = 'app-insights-${uniqueString(resourceGroup().id)}'
var storagefunctionName = 'stgfunc${uniqueString(resourceGroup().id)}'
var secretKeySearch = 'SEARCHSERVICESECRET'
var secretKeyStorageKey = 'STORAGEACCOUNTKEYSECRET'

var ipAddressToAllow = [
]

// Key Vault
module key_vault '../azresources/security/key-vault.bicep' = {
  name: 'key_vault'
  params: {
    keyVaultName: keyVaultName
    spnObjectId: spnObjectId
    app_services_website_principalId: app_service.outputs.app_services_website_principalId
    app_services_website_tenantId: app_service.outputs.app_services_website_tenantId
    app_services_function_app_principalId: function.outputs.app_services_function_app_principalId
    app_services_function_app_tenantId: function.outputs.app_services_function_app_tenantId
  }
}

module akv_secrets '../azresources/security/key-vault-secret.bicep' = {
  name: 'akv_secrets'
  dependsOn: [
    key_vault
  ]
  params: {
    akvName: keyVaultName
    azure_storage_account_data_id: azure_storage_account_data.outputs.azure_storage_account_data_id
    azure_storage_account_data_name: storageAccountNameData
    secretKeyStorageKey: secretKeyStorageKey
    secretKeySearch: secretKeySearch
    azure_search_service_id: azure_search_service.outputs.azure_search_service_id
    azure_cognitive_account_id: azure_cognitive_services.outputs.azure_congnitive_account_id
    azure_search_service_name: searchName
  }
}

// Search
module azure_search_service '../azresources/search/search.bicep' = {
  name: 'azure_search_service'
  params: {
    searchName: searchName
  }
}

// Cognitive Services

module azure_cognitive_services '../azresources/cognition/cognitive-services.bicep' = {
  name: 'azure_cognitive_services'
  params: {
    cognitiveAccountName: cognitiveAccountName
  }
}

module azure_storage_account_data '../azresources/storage/storage-account-app.bicep' = {
  name: 'azure_storage_account_data'
  params: {
    storageAccountNameData: storageAccountNameData
    ipAddressToAllow: ipAddressToAllow
    vnetId: vnetId
    subnetPrivateEndpointsName: subnetPrivateEndpointsName
    storageBlobPrivateZoneId: storageBlobPrivateZoneId
    docsContainerName: docsContainerName
    azure_search_service_principalId: azure_search_service.outputs.azure_search_service_principal_id
  }
}


module azure_storage_account_functions '../azresources/storage/storage-account-function.bicep' = {
  name: 'azure_storage_account_functions'
  params: {
    storagefunctionName: storagefunctionName
    vnetId: vnetId
    subnetPrivateEndpointsName: subnetPrivateEndpointsName
    storageBlobPrivateZoneId: storageBlobPrivateZoneId
  }
}

resource app_insights 'Microsoft.Insights/components@2015-05-01' = {
  name: appInsightsName
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

module app_service '../azresources/application/app-service.bicep' = {
  name: 'app_service'
  params: {
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    azure_search_service_name: searchName
    keyVaultName: keyVaultName
    secretKeySearch: secretKeySearch
    azure_storage_account_data_name: storageAccountNameData
    docsContainerName: docsContainerName
    vnetID: vnetId
    subnetAppServiceName: subnetAppServiceName
    secretKeyStorageKey: secretKeyStorageKey
    app_insightsInstrumentationKey: app_insights.properties.InstrumentationKey
  }
}

module function '../azresources/application/function.bicep' = {
  name: 'function'
  params: {
    functionAppName: functionAppName
    app_service_plan_id: app_service.outputs.app_service_plan_id
    app_insights_InstrumentationKey: app_insights.properties.InstrumentationKey
    azure_storage_account_functions_name: storagefunctionName
    azure_storage_account_functions_id: azure_storage_account_functions.outputs.azure_storage_account_functions_id
    vnetId: vnetId
    subnetAppServiceName: subnetAppServiceName
  }
}
