#!/bin/bash
## USAGE  ./deploy-infra.sh <LOCATION> <RESOURCE GROUP> <DEPLOYMENT TEMPLATE> <DOCS CONTAIRNER NAME> <SERVICE PRINCIPAL ID> <POWER BI WORKSPACE ID> <POWER BI REPORT ID> <POWER BI TENANT ID> <POWER BI CLIENT ID> <POWER BI CLIENT SECRET>
set -ex

location="$1"
resource_group="$2"
deployment_template="$3"
docs_container_name="$4"
service_principal_id="$5"
powerBi_workspace_id="$6"
powerBi_report_id="$7"
powerBi_tenant_id="$8"
powerBi_client_id="$9"
powerBi_client_secret="${10}"
powerBi_fallback_url="${11}"
use_existing_vnet="${12}"
vnet_name="${13}"
vnet_rg="${14}"

# Function that returns string1=string2 if string2 is not empty, otherwise returns empty string
function set_if_not_empty {
    if [[ -n $2 ]]; then
        echo "$1=$2"
    else
        echo ""
    fi
}

if [[ $(az group exists -n $resource_group) == 'false' ]]; then
    az group create -l $location -n $resource_group
fi

let "uniqueness=RANDOM*RANDOM"
readonly deployment_name=deploy-knowledge-mining-$uniqueness

array_of_parameters=("$(set_if_not_empty "docsContainerName" $docs_container_name)"
"$(set_if_not_empty "servicePrincipalId" $service_principal_id)"
"$(set_if_not_empty "powerBiWorkspaceId" $powerBi_workspace_id)"
"$(set_if_not_empty "powerBiReportId" $powerBi_report_id)"
"$(set_if_not_empty "powerBiTenantId" $powerBi_tenant_id)"
"$(set_if_not_empty "powerBiClientId" $powerBi_client_id)"
"$(set_if_not_empty "powerBiClientSecret" $powerBi_client_secret)"
"$(set_if_not_empty "powerBiFallbackUrl" $powerBi_fallback_url)"
"$(set_if_not_empty "useExistingVnet" $use_existing_vnet)"
"$(set_if_not_empty "vnetName" $vnet_name)"
"$(set_if_not_empty "vnetResourceGroup" $vnet_rg)")

az deployment group create -g $resource_group \
    -n $deployment_name \
    --template-file $deployment_template \
    --parameters "${array_of_parameters[@]}"

storage_account_id=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.storage_data_id.value -o tsv | tr -dc '[[:print:]]')
storage_account_name=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.storage_data_name.value -o tsv | tr -dc '[[:print:]]')
search_endpoint=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.search_endpoint.value -o tsv | tr -dc '[[:print:]]')
keyvault_name=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.keyvault_name.value -o tsv | tr -dc '[[:print:]]')
app_name=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.app_name.value -o tsv | tr -dc '[[:print:]]')
skills_name=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.skills_name.value -o tsv | tr -dc '[[:print:]]')

echo "::set-output name=storage_account_id::$storage_account_id"
echo "::set-output name=storage_account_name::$storage_account_name"
echo "::set-output name=search_endpoint::$search_endpoint"
echo "::set-output name=keyvault_name::$keyvault_name"
echo "::set-output name=app_name::$app_name"
echo "::set-output name=skills_name::$skills_name"

echo "##vso[task.setvariable variable=storage_account_id;isOutput=true]$storage_account_id"
echo "##vso[task.setvariable variable=storage_account_name;isOutput=true]$storage_account_name"
echo "##vso[task.setvariable variable=search_endpoint;isOutput=true]$search_endpoint"
echo "##vso[task.setvariable variable=keyvault_name;isOutput=true]$keyvault_name"
echo "##vso[task.setvariable variable=app_name;isOutput=true]$app_name"
echo "##vso[task.setvariable variable=skills_name;isOutput=true]$skills_name"
