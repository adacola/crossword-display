#!/bin/bash
set -eu
project_name=crossword-display
cd $(dirname $0)/..
docker build -t ${project_name} -f Dockerfile.build .
