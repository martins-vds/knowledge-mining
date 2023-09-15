#!/bin/bash
## USAGE  ./get-secret.sh <KEY VAULT NAME> <SECRET NAME>
set -e

function trim(){
    local string=$(echo $1 | tr -d '[[='"'"'=]]')

    echo "$string"
}

key_vault=$(trim "$1")
secret_name=$(trim "$2")

echo $(az keyvault secret show --vault-name $key_vault --name $secret_name --query value -o tsv)