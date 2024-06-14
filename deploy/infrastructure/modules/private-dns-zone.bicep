param vnetId string
param useExistingDnsZones bool = false
param resourceGroupName string
param subscriptionId string

var privateDnsZone = 'privatelink.blob.${environment().suffixes.storage}'

resource newDnsZone 'Microsoft.Network/privateDnsZones@2018-09-01' = if (!useExistingDnsZones) {
  name: privateDnsZone
  location: 'global'
}

module virtualLinkToNewDnsZone './private-dns-zone-link.bicep' = if (!useExistingDnsZones) {
  name: 'virtualLinkToNewDnsZone'  
  params: {
    vnetId: vnetId
    dnsZoneName: privateDnsZone
  }
}

module virtualLinkToExistingDnsZone './private-dns-zone-link.bicep' = if (useExistingDnsZones) {
  name: 'virtualLinkToExistingDnsZone'
  scope: resourceGroup(resourceGroupName, subscriptionId)
  params: {
    vnetId: vnetId
    dnsZoneName: privateDnsZone
  }
}

output id string = useExistingDnsZones ? virtualLinkToExistingDnsZone.outputs.id : virtualLinkToNewDnsZone.outputs.id
