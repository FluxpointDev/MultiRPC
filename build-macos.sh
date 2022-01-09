dotnet publish src/MultiRPC/MultiRPC.csproj -c Release -r osx-x64
dotnet publish src/MultiRPC/MultiRPC.csproj -c Release -r osx-arm64

cp -r -v ./macOS\ Templates/MultiRPC.app/ macOS\ Templates/MultiRPC-x64.app/
cp -r -v ./macOS\ Templates/MultiRPC.app/ macOS\ Templates/MultiRPC-arm64.app/
mkdir macOS\ Templates/MultiRPC-x64.app/Contents/MacOS/
mkdir macOS\ Templates/MultiRPC-arm64.app/Contents/MacOS/
cp -r ./src/MultiRPC/bin/Release/net6.0/osx-x64/publish/* macOS\ Templates/MultiRPC-x64.app/Contents/MacOS/
cp -r ./src/MultiRPC/bin/Release/net6.0/osx-arm64/publish/* macOS\ Templates/MultiRPC-arm64.app/Contents/MacOS/
