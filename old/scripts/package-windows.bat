set /p version=<version

mkdir ..\packages
cd "..\builds"

set filename=MultiRPC-Windows-x86-%version%
move "win-x86" "%filename%"
powershell.exe -nologo -noprofile -command "& { Compress-Archive -Path "%filename%" -DestinationPath "..\packages\%filename%" }"

set filename=MultiRPC-Windows-arm64-%version%
move "win-arm64" "%filename%"
powershell.exe -nologo -noprofile -command "& { Compress-Archive -Path "%filename%" -DestinationPath "..\packages\%filename%" }"