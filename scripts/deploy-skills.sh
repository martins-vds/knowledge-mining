#!/bin/bash
## USAGE  ./deploy-app.sh <RG NAME> <APPSERVICE NAME> <SYNMAPS STORAGE ACCOUNT> <ZIP PACKAGE>
set -ex #echo on

# Define variables

rg_name="$1"
app_service_name="$2"
synmaps_storage_account="$3"
zip_package="$4"

# Deploy to App Service Package
az webapp config appsettings set --resource-group $rg_name --name $app_service_name --settings ENABLE_ORYX_BUILD="false"
az webapp config appsettings set --resource-group $rg_name --name $app_service_name --settings SynonymsStorage=$synmaps_storage_account
az functionapp deployment source config-zip -g $rg_name -n $app_service_name  --src $zip_package