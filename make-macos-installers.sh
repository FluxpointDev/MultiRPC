#!/bin/sh
cp "macOS-Templates/MultiRPC_Base.dmg" "macOS-Templates/MultiRPC_Base-x64.dmg"
cp "macOS-Templates/MultiRPC_Base.dmg" "macOS-Templates/MultiRPC_Base-arm64.dmg"
sh make-macos-installer.sh "macOS-Templates/MultiRPC_Base-x64.dmg" "macOS-Templates/MultiRPC_Installer-x64.dmg" "macOS-Templates/MultiRPC-x64.app"
sh make-macos-installer.sh "macOS-Templates/MultiRPC_Base-arm64.dmg" "macOS-Templates/MultiRPC_Installer-arm64.dmg" "macOS-Templates/MultiRPC-arm64.app"