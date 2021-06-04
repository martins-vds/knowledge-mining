// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param keyVaultName string
param spnObjectId string
param app_services_website_principalId string
param app_services_website_tenantId string
param app_services_function_app_principalId string
param app_services_function_app_tenantId string

resource azure_key_vault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: keyVaultName
  location: resourceGroup().location
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
    accessPolicies: [
      {
        objectId: spnObjectId
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
        objectId: app_services_website_principalId
        tenantId: app_services_website_tenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
      {
        objectId: app_services_function_app_principalId
        tenantId: app_services_function_app_tenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ]
  }
}
