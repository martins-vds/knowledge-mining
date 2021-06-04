// ----------------------------------------------------------------------------------
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

targetScope = 'subscription'

param azureRegion string = deployment().location

param docsContainerName string = 'documents'
param spnObjectId string = '2314dacf-58d3-42b4-8943-386da055fd35'

param tagClientOrganization string = ''
param tagCostCenter string = ''
param tagDataSensitivity string = ''
param tagProjectContact string = ''
param tagProjectName string = ''
param tagTechnicalContact string = ''

param rgSearchAppName string = 'myapp6'
param rgNetworkName string = 'azmlNetwork6656'
param vnetName string = 'vnet'

param subnetAppServiceName string = 'AppService'
param subnetAppServicePrefix string = '10.0.10.0/25'

param subnetPrivateEndpointsName string = 'privateendpoints'
param subnetPrivateEndpointPrefix string = '10.0.8.0/25'

param deploySubnetsInExistingVnet bool = true
param vnetAddressSpace string = '10.0.0.0/16'

var tags = {
  ClientOrganization: tagClientOrganization
  CostCenter: tagCostCenter
  DataSensitivity: tagDataSensitivity
  ProjectContact: tagProjectContact
  ProjectName: tagProjectName
  TechnicalContact: tagTechnicalContact
}

resource rgSearchApp 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: rgSearchAppName
  location: azureRegion
  tags: tags
}

resource rgNetworkingExisting 'Microsoft.Resources/resourceGroups@2020-06-01' existing = if(deploySubnetsInExistingVnet) {
  name: rgNetworkName
}

resource rgNetworkingNew 'Microsoft.Resources/resourceGroups@2020-06-01' = if (!deploySubnetsInExistingVnet) {
  name: rgNetworkName
  location: azureRegion
  tags: tags
}



module vnetExisting 'networking.bicep' = if(deploySubnetsInExistingVnet) {
  name: 'vnetExisting'
  scope: rgNetworkingExisting
  params: {
    vnetName: vnetName
    subnetAppServiceName: subnetAppServiceName
    subnetAppServicePrefix: subnetAppServicePrefix
    deploySubnetsInExistingVnet: deploySubnetsInExistingVnet
    vnetAddressSpace: vnetAddressSpace
    subnetPrivateEndpointsName: subnetPrivateEndpointsName
    subnetPrivateEndpointPrefix: subnetPrivateEndpointPrefix
  }
}

module vnetNew 'networking.bicep' = if(!deploySubnetsInExistingVnet) {
  name: 'vnetNew'
  scope: rgNetworkingNew
  params: {
    vnetName: vnetName
    subnetAppServiceName: subnetAppServiceName
    subnetAppServicePrefix: subnetAppServicePrefix
    deploySubnetsInExistingVnet: deploySubnetsInExistingVnet
    vnetAddressSpace: vnetAddressSpace
    subnetPrivateEndpointsName: subnetPrivateEndpointsName
    subnetPrivateEndpointPrefix: subnetPrivateEndpointPrefix
  }
}


module addon 'env-vnet-integration.bicep' = {
  name: 'searchapp-add-on'
  scope: rgSearchApp
  params: {
    docsContainerName: docsContainerName
    spnObjectId: spnObjectId
    subnetAppServiceName: subnetAppServiceName
    subnetPrivateEndpointsName: subnetPrivateEndpointsName
    vnetId: deploySubnetsInExistingVnet ? vnetExisting.outputs.vnetID : vnetNew.outputs.vnetID
    storageBlobPrivateZoneId: deploySubnetsInExistingVnet ? vnetExisting.outputs.storageBlobPrivateZoneId : vnetNew.outputs.storageBlobPrivateZoneId
  }
}









