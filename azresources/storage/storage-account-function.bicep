// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param vnetId string
param subnetPrivateEndpointsName string
param storageBlobPrivateZoneId string
param storagefunctionName string

resource azure_storage_account_functions 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: storagefunctionName
  location: resourceGroup().location
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
      bypass: 'Logging,Metrics'
    }
  }
}

resource azure_storage_account_functions_blob_pe 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  location: resourceGroup().location
  name: '${azure_storage_account_functions.name}-blob-endpoint'
  properties: {
    subnet: {
      id: '${vnetId}/subnets/${subnetPrivateEndpointsName}'
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

resource azure_storage_account_functions_blob_pe_dns_reg 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-06-01' = {
  name: '${azure_storage_account_functions_blob_pe.name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink_blob_core_windows_net'
        properties: {
          privateDnsZoneId: storageBlobPrivateZoneId
        }
      }
    ]
  }
}

output azure_storage_account_functions_id string = azure_storage_account_functions.id
