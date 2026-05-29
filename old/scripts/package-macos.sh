#!/bin/bash
set -e
set -o pipefail

version=`cat version`
# $1=os $2=arch $3=ext
getfilelocation() 
{
    filename="MultiRPC-${1}-${2}-${version}" 
}

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