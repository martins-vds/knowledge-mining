param dnsZoneName string
param vnetId string

resource dnsZone 'Microsoft.Network/privateDnsZones@2018-09-01' existing = {
  name: dnsZoneName  
}

resource link 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: dnsZone
  name: uniqueString(vnetId)  
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId
    }
    registrationEnabled: false
  }
}

output id string = link.id
