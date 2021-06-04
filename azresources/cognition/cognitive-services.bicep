// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------


param cognitiveAccountName string

resource azure_congnitive_account 'Microsoft.CognitiveServices/accounts@2017-04-18' = {
  name: cognitiveAccountName
  location: resourceGroup().location
  kind: 'CognitiveServices'

  sku: {
    name: 'S0'
  }
}

output azure_congnitive_account_id string = azure_congnitive_account.id
