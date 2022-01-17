#!/bin/sh
# Mount base .dmg

path=$PWD
hdiutil attach "$path/${1}"
cd "/Volumes/MultiRPC Installer/"
# Remove old files
rm -rf "MultiRPC.app/*"
rm -rf "MultiRPC.app/Contents"
# Copy new files into "MultiRPC.app"
cp -r "$path/${3}/Contents" "/Volumes/MultiRPC Installer/MultiRPC.app/Contents"
codesign --force --deep --sign - "$path/${3}"
# Unmount base .dmg
cd ../
hdiutil detach "/Volumes/MultiRPC Installer/"
# Create readonly compressed version
rm "$path/${2}"
hdiutil convert "$path/${1}" -format UDZO -o "$path/${2}"
# $1: Base file, $2: Installer file, $3: .app locations
