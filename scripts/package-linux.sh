#!/bin/bash
set -e
set -o pipefail

echo "Building for all OS's"
sh build-all.sh && echo "Built for all OS's" || (echo "Wasn't able to build for all OS's, stopping..."; exit -1)

version=`cat version`
# $1=os $2=arch $3=ext
getfilelocation() 
{
    filename="MultiRPC-${1}-${2}-${version}" 
}

mkdir ../packages || echo "folder already exists"
cd "../builds"

# Package Linux
getfilelocation "Linux" "arm"
mv "linux-arm" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"

getfilelocation "Linux" "x64"
mv "linux-x64" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"