#!/bin/bash
set -ex

dotnet publish ./app/src/KnowledgeMining.Functions.Skills/KnowledgeMining.Functions.Skills.csproj -c Release -o ./dist/skills/linux-x64/ -r linux-x64 -f 'net6.0' --self-contained

pushd ./dist/skills/linux-x64

zip -r -q -9 ../../skills.linux-x64.zip *

popd

rm -rf ./dist/skills