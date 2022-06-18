#!/bin/bash
set -eux

# paket
dotnet tool restore

if [[ -f "paket.lock" ]]; then
    dotnet paket restore
fi
