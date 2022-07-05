#!/bin/bash
## USAGE  ./deploy-app.sh <RG NAME> <APPSERVICE NAME> <ZIP PACKAGE>
set -ex #echo on

function trim(){
    local string=$(echo $1 | tr -d '[[='"'"'=]]')

    echo "$string"
}

# Define variables

rg_name=$(trim "$1")
app_service_name=$(trim "$2")
zip_package=$(trim "$3")

# Deploy to App Service Package
az webapp config appsettings set --resource-group $rg_name --name $app_service_name --settings ENABLE_ORYX_BUILD="false"
az functionapp deployment source config-zip -g $rg_name -n $app_service_name  --src $zip_package

