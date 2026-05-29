#!/bin/bash
set -e
set -o pipefail

# Build
echo "Building macOS x64"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r osx-x64 -o ../builds/osx-x64 --self-contained && echo "Built macOS x64" || ("Failed to build for macOS x64, stopping..." && exit -1)
echo "Building macOS arm64 (M1)"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r osx-arm64 -o ../builds/osx-arm64 --self-contained && echo "Built macOS arm64 (M1)" || ("Failed to build for macOS arm64 (M1), stopping..." && exit -1)

# Add into .app's
echo "Packaging .app's"
mkdir -p ../builds/MultiRPC-x64.app/Contents/MacOS/ && echo "Setup .app for x64" || ("Failed to setup .app for macOS x64, stopping..." && exit -1)
mkdir -p ../builds/MultiRPC-arm64.app/Contents/MacOS/ && echo "Setup .app for arm64 (M1)" || ("Failed to setup .app for arm64 (M1), stopping..." && exit -1)
cp -r ../macOS-Templates/MultiRPC.app/* ../builds/MultiRPC-x64.app/ && echo "Copied base .app for x64" || ("Failed copy base .app for x64, stopping..." && exit -1)
cp -r ../macOS-Templates/MultiRPC.app/* ../builds/MultiRPC-arm64.app/ && echo "Copied base .app for arm64 (M1)" || ("Failed to copy base .app for arm64 (M1), stopping..." && exit -1)
cp -r ../builds/osx-x64/* ../builds/MultiRPC-x64.app/Contents/MacOS/ && echo "Put x64 files in for x64 .app" || ("Failed to put x64 files in for x64 .app, stopping..." && exit -1)
cp -r ../builds/osx-arm64/* ../builds/MultiRPC-arm64.app/Contents/MacOS/ && echo "Put arm64 files in for arm64 (M1) .app" || ("Failed to put arm64 files in for arm64 (M1) .app, stopping..." && exit -1)

if [[ $OSTYPE != 'darwin'* ]] ; then
    echo "We can't sign the application as we are not on macOS, skipping..."
    exit 0
fi

# Sign the files, else we will get "this application 
# is damaged" when someone downloads the application and prevent from the M1 build from even starting up
codesign --force --deep --sign - ../builds/MultiRPC-arm64.app && echo "Signed arm64 (M1) .app" || ("Failed to sign arm64 (M1) .app, stopping..." && exit -1)
codesign --force --deep --sign - ../builds/MultiRPC-x64.app && echo "Signed x64 .app" || ("Failed to sign x64 .app, stopping..." && exit -1)