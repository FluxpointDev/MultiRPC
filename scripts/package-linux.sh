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

# Package Linux - Arch

./create_arch.sh aarch64
./create_arch.sh arm
./create_arch.sh x86_64

# Package Linux - Debian and friends

cd "../builds"
getfilelocation "Linux" "arm"
mv "linux-arm" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"
cd ../scripts
sh create_deb.sh ../builds/$filename arm multirpc-arm

cd "../builds"
getfilelocation "Linux" "arm64"
mv "linux-arm64" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"
cd ../scripts
sh create_deb.sh ../builds/$filename arm multirpc-arm64

cd "../builds"
getfilelocation "Linux" "x64"
mv "linux-x64" "${filename}"
tar -C "../builds" -czvf "../packages/${filename}.tar.gz" "$filename"
cd ../scripts
sh create_deb.sh ../builds/$filename amd64 multirpc-x64
