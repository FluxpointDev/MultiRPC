#!/bin/bash
set -e
set -o pipefail

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
cd ../scripts
sh create_deb.sh ../builds/$filename arm multirpc-arm

cd "../builds"
getfilelocation "Linux" "x64"
mv "linux-x64" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"
cd ../scripts
sh create_deb.sh ../builds/$filename amd64 multirpc-x64
