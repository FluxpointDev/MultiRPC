#!/bin/bash
set -e
set -o pipefail

if [[ $OSTYPE != 'darwin'* ]] ; then
    echo 'This script can only be ran on macOS due to tooling that is only around in macOS!'
    exit 0
fi

path=$PWD

# Mount base .dmg
hdiutil attach "$path/${1}" && echo "Attached base installer" || (echo "Failed to attach base installer, stopping..."; exit -1)
cd "/Volumes/MultiRPC Installer/" || (echo "MultiRPC Installer folder doesn't exit"; exit -1)

fail()
{ 
    echo "${1}, stopping..."; 
    cd $path; 
    hdiutil detach "/Volumes/MultiRPC Installer/"; 
    exit -1 
}

# Remove old files
rm -rf "MultiRPC.app/*" || fail "Wasn't able to remove old MultiRPC files (Root)"
rm -rf "MultiRPC.app/Contents" || fail "Wasn't able to remove old MultiRPC files (Contents)"

# Copy new files into "MultiRPC.app" and sign the files, else we will get "this application 
# is damaged" when someone downloads the application and prevent from the M1 build from even starting up
cp -r "$path/${3}/Contents" "/Volumes/MultiRPC Installer/MultiRPC.app/Contents" || fail "Wasn't able to add new MultiRPC files"
codesign --force --deep --sign - "$path/${3}" || fail "Wasn't able to sign .app"

# Unmount base .dmg
cd ../
hdiutil detach "/Volumes/MultiRPC Installer/"

# Create readonly compressed version
rm "$path/${2}" || echo "Older Installer file didn't exist"
hdiutil convert "$path/${1}" -format UDZO -o "$path/${2}" || echo "Wasn't able compress installer file..."

# $1: Base file, $2: Installer file, $3: .app locations