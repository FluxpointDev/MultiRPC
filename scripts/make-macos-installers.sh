#!/bin/bash
set -e
set -o pipefail

echo "Copying base installer files"
cp "../macOS-Templates/MultiRPC_Base.dmg" "../builds/MultiRPC_Base-x64.dmg" && echo "Copied base installer file for x64" || (echo "Failed to copy base installer file for x64"; exit -1)
cp "../macOS-Templates/MultiRPC_Base.dmg" "../builds/MultiRPC_Base-arm64.dmg" && echo "Copied base installer file for arm64 (M1)" || (echo "Failed to copy base installer file for arm64 (M1)"; exit -1)
echo "Copied base installer files"

echo "Making Installers"
sh make-macos-installer.sh "../builds/MultiRPC_Base-x64.dmg" "../builds/MultiRPC_Installer-x64.dmg" "../builds/MultiRPC-x64.app" && echo "Made x64 installer" || (echo "Failed to make x64 installer"; exit -1)
sh make-macos-installer.sh "../builds/MultiRPC_Base-arm64.dmg" "../builds/MultiRPC_Installer-arm64.dmg" "../builds/MultiRPC-arm64.app" && echo "Made arm64 (M1) installer" || (echo "Failed to make arm64 (M1) installer"; exit -1)
echo "Made Installers"

echo "Removing base installer files"
rm ../builds/MultiRPC_Base-x64.dmg || echo "Unable to remove x64 base installer file"
rm ../builds/MultiRPC_Base-arm64.dmg || echo "Unable to remove arm64 base installer file"
echo "Removed base installer files"