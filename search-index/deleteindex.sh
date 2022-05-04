#!/bin/bash
set -x

# Constants
SEARCH_SERVICE_APIVERSION="2021-04-30-Preview"


INDEX_NAME="km"
INDEXER_NAME="km-indexer"
DATA_SOURCE_NAME="azstorage"
SKILLS_NAME="km-skills"
SYNMAP_NAME="km-synmap"


# Delete Indexer
curl --request DELETE \
    --url "${SEARCH_SERVICE_ENDPOINT}/indexers/${INDEXER_NAME}/?api-version=${SEARCH_SERVICE_APIVERSION}" \
    --header 'Content-Type: application/json' \
    --header "api-key: ${SEARCH_SERVICE_SECRET}" \
    --header 'cache-control: no-cache' \
  

# Create SlilleSet
curl --request DELETE \
      --url "${SEARCH_SERVICE_ENDPOINT}/skillsets/${SKILLS_NAME}/?api-version=${SEARCH_SERVICE_APIVERSION}" \
      --header 'Content-Type: application/json' \
      --header "api-key: ${SEARCH_SERVICE_SECRET}" \
      --header 'cache-control: no-cache' \


curl --request DELETE \
      --url "${SEARCH_SERVICE_ENDPOINT}/indexes/${INDEX_NAME}/?api-version=${SEARCH_SERVICE_APIVERSION}" \
      --header 'Content-Type: application/json' \
      --header "api-key: ${SEARCH_SERVICE_SECRET}" \
      --header 'cache-control: no-cache' \
   
