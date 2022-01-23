#!/bin/bash
set -e
set -o pipefail

echo "Building Windows x86"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r win-x86 -o ../builds/win-x86 --self-contained && echo "Built Windows x86" || (echo "Failed to build for Windows x86"; exit -1)
echo "Building Windows arm"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r win-arm -o ../builds/win-arm --self-contained && echo "Built Windows arm" || (echo "Failed to build for Windows arm"; exit -1)