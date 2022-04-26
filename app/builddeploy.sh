#!/bin/bash
## USAGE  ./builddeploy.sh <RG NAME> <APPSERVICE NAME>
set -x #echo on

# Constants
NETCORE_VERSION="netcoreapp3.1"

# Define variables

RGNAME="$1"
APPSERVICENAME="$2"

# Build the app and prepare deployment
dotnet publish -c Release -f netcoreapp3.1 -o publish

pushd publish

# Zip Package
zip -r app.zip ./

# Deploy to App Service Package
az webapp config appsettings set --resource-group $RGNAME --name $APPSERVICENAME --settings ENABLE_ORYX_BUILD="false"
az webapp deploy --resource-group $RGNAME --name $APPSERVICENAME --src-path app.zip --type zip --clean true --restart true

popd