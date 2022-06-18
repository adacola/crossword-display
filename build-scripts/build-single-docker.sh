#!/bin/bash
set -eu
echo "mcr.microsoft.com/dotnet/sdk:6.0 dockerイメージが存在することを前提としています。存在しない場合は以下のコマンドでイメージをpullしてください。"
echo "$ docker pull mcr.microsoft.com/dotnet/sdk:6.0"
dir=$(cd $(dirname $0)/.. && pwd)
docker run --rm -it -v ${dir}:/work mcr.microsoft.com/dotnet/sdk:6.0 /work/build-scripts/build-single.sh
