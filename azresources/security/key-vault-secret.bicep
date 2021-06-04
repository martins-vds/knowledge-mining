// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param akvName string
param azure_storage_account_data_id string
param azure_storage_account_data_name string
param secretKeyStorageKey string
param secretKeySearch string
param azure_search_service_id string
param azure_search_service_name string
param azure_cognitive_account_id string


resource akv_secret_storage_account_resource_id 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/STORAGEACCOUNTRESOURCEID'
  properties: {
    value: azure_storage_account_data_id
  }
}

resource akv_secret_storage_account_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/STORAGEACCOUNTCONNECTIONSTRING'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${azure_storage_account_data_name};AccountKey=${listKeys(azure_storage_account_data_id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
  }
}

resource akv_secret_storage_account_key_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/${secretKeyStorageKey}'
  properties: {
    value: listKeys(azure_storage_account_data_id, '2019-06-01').keys[0].value
  }
}

resource akv_secret_search_endpoint 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/SEARCHSERVICEENDPOINT'
  properties: {
    value: 'https://${azure_search_service_name}.search.windows.net'
  }
}

resource akv_secret_search_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/${secretKeySearch}'
  properties: {
    value: listAdminKeys(azure_search_service_id, '2020-08-01').primaryKey
  }
}

resource akv_secret_cognitive_services_secret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  name: '${akvName}/COGNITIVESERVICESSECRET'
  properties: {
    value: listKeys(azure_cognitive_account_id, '2017-04-18').key1
  }
}
