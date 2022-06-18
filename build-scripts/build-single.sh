#!/bin/bash
set -eu
cd $(dirname $0)/../crossword-display
dotnet tool restore
dotnet paket restore
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true
