echo "Building Windows x86"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r win-x86 -o ../builds/win-x86 --self-contained && echo "Built Windows x86" || (echo "Failed to build for Windows x86"; exit -1)
robocopy %userprofile%\.nuget\packages\avalonia.angle.windows.natives\2.1.0.2020091801\runtimes\win7-x86\native\ ../builds/win-x86/

echo "Building Windows arm64"
dotnet publish ../src/MultiRPC/MultiRPC.csproj -c Release -r win-arm64 -o ../builds/win-arm64 --self-contained && echo "Built Windows arm64" || (echo "Failed to build for Windows arm64"; exit -1)
robocopy %userprofile%\.nuget\packages\avalonia.angle.windows.natives\2.1.0.2020091801\runtimes\win-arm64\native\ ../builds/win-arm64/