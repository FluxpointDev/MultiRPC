#!/bin/bash
set -e
set -o pipefail

echo "Building for all OS's"
sh build-all.sh && echo "Built for all OS's" || (echo "Wasn't able to build for all OS's, stopping..."; exit -1)

version="v7-beta7"

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

# Package Windows
getfilelocation "Windows" "x86"
mv "win-x86" "${filename}"
zip -r "../packages/${filename}.zip" "${filename}"

getfilelocation "Windows" "arm"
mv "win-arm" "${filename}"
zip -r "../packages/${filename}.zip" "${filename}"

# Package macOS
getfilelocation "macOS" "x64"
mv "MultiRPC-x64.app" "MultiRPC.app"
zip -r "../packages/${filename}.zip" "MultiRPC.app"
mv "MultiRPC.app" "MultiRPC-x64.app"

mv "MultiRPC-arm64.app" "MultiRPC.app"
getfilelocation "macOS" "arm64"
zip -r "../packages/${filename}.zip" "MultiRPC.app"
mv "MultiRPC.app" "MultiRPC-arm64.app"

# Copy macOS install files if we are on macOS
if [[ $OSTYPE != 'darwin'* ]]; then
    echo "We won't have the installer files due to not being on macOS"
    exit 0
fi

getfilelocation "macOS" "x64"
cp "MultiRPC_Installer-x64.dmg" "../packages/${filename}.dmg"

getfilelocation "macOS" "arm64"
cp "MultiRPC_Installer-arm64.dmg" "../packages/${filename}.dmg"