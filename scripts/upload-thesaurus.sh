#!/bin/bash
## USAGE  ./upload-thesaurus.sh <RESOURCE GROUP> <STORAGE ACCOUNT NAME> <SYNONMS CONTAINER> <THESAURUS FILE PATH>
set -ex

function trim(){
    local string=$(echo $1 | tr -d '[[='"'"'=]]')

    echo "$string"
}

rg_name=$(trim "$1")
storage_account_name=$(trim "$2")
synonyms_container=$(trim "$3")
thesaurus_file_path=$(trim "$4")

ip_address=$(host myip.opendns.com resolver1.opendns.com | grep "myip.opendns.com has" | awk '{print $4}')

az storage account network-rule add -g $rg_name --account-name $storage_account_name --ip-address $ip_address

sleep 10

az storage blob upload --account-name $storage_account_name -f $thesaurus_file_path -c $synonyms_container -n thesaurus.json --overwrite

az storage account network-rule remove -g $rg_name --account-name $storage_account_name --ip-address $ip_address