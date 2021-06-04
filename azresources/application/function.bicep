// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param functionAppName string
param app_service_plan_id string
param app_insights_InstrumentationKey string
param azure_storage_account_functions_name string
param azure_storage_account_functions_id string
param vnetId string
param subnetAppServiceName string

resource app_services_function_app 'Microsoft.Web/sites@2020-06-01' = {
  name: functionAppName
  location: resourceGroup().location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: app_service_plan_id
    siteConfig: {
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
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'node'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '10.14.1'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: app_insights_InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${azure_storage_account_functions_name};AccountKey=${listKeys(azure_storage_account_functions_id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
        }
      ]
    }
  }
}

resource app_services_function_app_vnet 'Microsoft.Web/sites/networkConfig@2020-06-01' = {
  name: '${app_services_function_app.name}/VirtualNetwork'
  properties: {
    subnetResourceId: '${vnetId}/subnets/${subnetAppServiceName}'
    swiftSupported: true
  } 
}

output app_services_function_app_principalId string = app_services_function_app.identity.principalId
output app_services_function_app_tenantId string = app_services_function_app.identity.tenantId
