#!/bin/bash
## USAGE  ./deploy-infra.sh <LOCATION> <RESOURCE GROUP> <DEPLOYMENT TEMPLATE> <DOCS CONTAIRNER NAME> <SERVICE PRINCIPAL ID>
set -ex

location="$1"
resource_group="$2"
deployment_template="$3"
docs_container_name="$4"
service_principal_id="$5"

if [[ $(az group exists -n $resource_group) == 'false' ]]; then
    az group create -l $location -n $resource_group
fi

let "uniqueness=RANDOM*RANDOM"
readonly deployment_name=deploy-knowledge-mining-$uniqueness

az deployment group create -g $resource_group \
                           -n $deployment_name \
                           --template-file $deployment_template \
                           --parameters docsContainerName=$docs_container_name \
                                        servicePrincipalId=$service_principal_id

storage_account_id=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.storage_data_id.value -o tsv)
search_enpoint=$(az deployment group show -g $resource_group -n $deployment_name --query properties.outputs.search_enpoint.value -o tsv)

echo "::set-output name=storage_account_id::$storage_account_id"
echo "::set-output name=search_enpoint::$search_enpoint"