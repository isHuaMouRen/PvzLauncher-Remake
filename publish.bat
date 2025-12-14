@echo off
setlocal enabledelayedexpansion

dotnet publish "PvzLauncherRemake\PvzLauncherRemake.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"
dotnet publish "UpdateService\UpdateService.csproj" -c Release -o "publish\bin"


set /p majorversion=MajorVersion: 
set /p version=Version: 
set /p codename=CodeName: 

set "targetDir=Builds\%majorversion%\%codename%\%version%"

mkdir "%targetDir%" 2>nul

xcopy "publish\*.*" "%targetDir%\" /s /e /y /i

del "Builds\%majorVersion%\%codename%\%version%\PvzLauncher.deps.json"
del "Builds\%majorVersion%\%codename%\%version%\PvzLauncher.pdb"

pause