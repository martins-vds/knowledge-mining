param vnetId string
var privateDnsZone = 'privatelink.blob.${environment().suffixes.storage}'

resource dnsZone 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZone
  location: 'global'
}

resource virtualLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
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

output id string = virtualLink.id
