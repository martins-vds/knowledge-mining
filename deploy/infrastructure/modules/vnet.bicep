param name string
param location string
param useExistingVnet bool = false
param vnetResourceGroup string

var subnetAppServiceName = 'AppService'
var subnetPrivateEndpointsName = 'PrivateEndpoints'

// Networking
resource newVnet 'Microsoft.Network/virtualNetworks@2020-06-01' = if (!useExistingVnet) {
  location: location
  name: name
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetAppServiceName
        properties: {
          addressPrefix: '10.0.1.0/24'
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
      {
        name: subnetPrivateEndpointsName
        properties: {
          addressPrefix: '10.0.2.0/24'
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

resource existingVnet 'Microsoft.Network/virtualNetworks@2020-06-01' existing = if (useExistingVnet) {
  name: name
  scope: resourceGroup(vnetResourceGroup)
}

output id string = useExistingVnet ? existingVnet.id : newVnet.id
