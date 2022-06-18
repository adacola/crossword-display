#!/bin/bash
set -eu
project_name=crossword-display
dir=$(dirname $0)
fullpath_dir=$(readlink -f $dir)
dir_name=$(basename $fullpath_dir)
datetime=$(date "+%Y%m%d-%H%M%S")
pack_file=${project_name}-${datetime}.tar.gz
cd $dir
git clean -Xdf
cd ..
tar -cvzf ${pack_file} --exclude-vcs ${dir_name}/
mkdir -p ${dir_name}/pack
mv ${pack_file} ${dir_name}/pack/
