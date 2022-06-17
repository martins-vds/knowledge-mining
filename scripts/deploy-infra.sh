#!/bin/bash
## USAGE  ./deploy-infra.sh <LOCATION> <RESOURCE GROUP> <DEPLOYMENT TEMPLATE> <DOCS CONTAIRNER NAME> <SPN OBJECT ID>
set -x #echo on

LOCATION="$1"
RESOURCEGROUP="$2"
DEPLOYMENTTEMPLATE="$3"
DOCSCONTAINERNAME="$4"
SPNOBJECTID="$5"

az group create -l $LOCATION -n $RESOURCEGROUP

az deployment group create -g $RESOURCEGROUP \
                            --template-file $DEPLOYMENTTEMPLATE \
                            --parameters docsContainerName=$DOCSCONTAINERNAME \
                                         spnObjectId=$SPNOBJECTID