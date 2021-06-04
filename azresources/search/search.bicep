// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------


param searchName string


resource azure_search_service 'Microsoft.Search/searchServices@2020-08-01' = {
  name: searchName
  location: resourceGroup().location
  sku: {
    name: 'standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output azure_search_service_id string = azure_search_service.id
output azure_search_service_principal_id string = azure_search_service.identity.principalId
