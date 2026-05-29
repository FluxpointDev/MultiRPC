#!/bin/bash
set -e
set -o pipefail

echo "Building for Windows"
sh build-windows.sh && echo "Built for Windows" || (echo "Wasn't able to build for Windows, stopping..."; exit -1)
echo "Building for Linux"
sh build-linux.sh && echo "Built for Linux" || (echo "Wasn't able to build for Linux, stopping..."; exit -1)
echo "Building for macOS"
sh build-macos.sh && echo "Built for macOS" || (echo "Wasn't able to build for macOS, stopping..."; exit -1)

if [[ $OSTYPE != 'darwin'* ]] ; then
    echo 'We can only make the installers on macOS due to tooling only on macOS, skipping...'
    exit 0
fi
sh make-macos-installers.sh && echo "Finished making installers for macOS" || (echo "Failed to make installers for macOS"; exit -1)