// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param storageAccountNameData string
param ipAddressToAllow array
param vnetId string
param subnetPrivateEndpointsName string
param storageBlobPrivateZoneId string
param docsContainerName string
param azure_search_service_principalId string

// Storage Accounts
resource azure_storage_account_data 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: storageAccountNameData
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
      bypass: 'AzureServices,Logging,Metrics'
      ipRules: ipAddressToAllow
    }
  }
}

resource azure_storage_account_data_blob_pe 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  location: resourceGroup().location
  name: '${azure_storage_account_data.name}-blob-endpoint'
  properties: {
    subnet: {
      id: '${vnetId}/subnets/${subnetPrivateEndpointsName}'
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
          privateDnsZoneId: storageBlobPrivateZoneId
        }
      }
    ]
  }
}

resource azure_storage_account_container_docs 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01'  = {
  name: '${azure_storage_account_data.name}/default/${docsContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

// Role Assignments
resource roleAssignSearchToStorageBlobReader 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, 'Storage Blob Data Reader')
  scope: azure_storage_account_data
  properties: {
    roleDefinitionId: '${subscription().id}/providers/Microsoft.Authorization/roleDefinitions/2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
    principalId: azure_search_service_principalId
    principalType: 'ServicePrincipal'
  }
}

output azure_storage_account_data_id string = azure_storage_account_data.id
