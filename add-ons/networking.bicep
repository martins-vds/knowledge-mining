// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

param vnetName string
param deploySubnetsInExistingVnet bool

param subnetAppServiceName string
param subnetAppServicePrefix string
param vnetAddressSpace string
param subnetPrivateEndpointsName string
param subnetPrivateEndpointPrefix string

resource vnetNew 'Microsoft.Network/virtualNetworks@2020-06-01' = if (!deploySubnetsInExistingVnet) {
  name: vnetName
  location: resourceGroup().location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressSpace
      ]
    }
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2020-11-01' existing = if (deploySubnetsInExistingVnet) {
  name: vnetName
}

resource subnetAppService 'Microsoft.Network/virtualNetworks/subnets@2020-07-01' = {
  name: '${!deploySubnetsInExistingVnet ? vnetNew.name : vnet.name}/${subnetAppServiceName}'
  properties: {
    addressPrefix: subnetAppServicePrefix
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

resource subnetPrivateEndpoints'Microsoft.Network/virtualNetworks/subnets@2020-07-01' = {
  dependsOn: [
    subnetAppService
  ]
  name: '${!deploySubnetsInExistingVnet ? vnetNew.name : vnet.name}/${subnetPrivateEndpointsName}'
  properties: {
    addressPrefix: subnetPrivateEndpointPrefix
    privateEndpointNetworkPolicies: 'Disabled'
  }
}

resource storageBlobPrivateZone 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: 'privatelink.blob.core.windows.net'
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

output vnetID string = deploySubnetsInExistingVnet ? vnet.id : vnetNew.id
output storageBlobPrivateZoneId string = storageBlobPrivateZone.id
