#!/bin/bash
set -e
set -o pipefail

echo "Building Linux x64"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r linux-x64 -o ../builds/linux-x64 --self-contained && echo "Built Linux x64" || (echo "Failed to build for Linux x64"; exit -1)
echo "Building Linux arm"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r linux-arm -o ../builds/linux-arm --self-contained && echo "Built Linux arm" || (echo "Failed to build for Linux arm"; exit -1)