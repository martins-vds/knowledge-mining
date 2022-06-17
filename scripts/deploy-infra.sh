#!/bin/bash
## USAGE  ./deploy-infra.sh <LOCATION> <RESOURCE GROUP> <DEPLOYMENT TEMPLATE> <DOCS CONTAIRNER NAME> <SERVICE PRINCIPAL ID>
set -x

location="$1"
resource_group="$2"
deployment_template="$3"
docs_container_name="$4"
service_principal_id="$5"

if [[ $(az group exists -n $resource_group) == 'false' ]]; then
    az group create -l $location -n $resource_group
fi

let "uniqueness=RANDOM*RANDOM"

az deployment group create -g $resource_group \
                           -n deploy-knowledge-mining-$uniqueness \
                           --template-file $deployment_template \
                           --parameters docsContainerName=$docs_container_name \
                                        servicePrincipalId=$service_principal_id
