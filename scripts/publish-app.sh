#!/bin/bash
set -ex

dotnet publish ./app/KnowledgeMining.UI/KnowledgeMining.UI.csproj -c Release -o ./dist/linux-x64/ -r linux-x64

pushd ./dist/linux-x64

zip -r -q -9 ../app.linux-x64.zip *

popd