@echo off
setlocal enabledelayedexpansion

dotnet publish "PvzLauncherRemake\PvzLauncherRemake.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"
dotnet publish "UpdateService\UpdateService.csproj" -c Release -o "publish\bin"


set "DEFAULT_MAJORVERSION=1.0.0"
set "DEFAULT_CODENAME=Release Candidate"

set /p version=Version: 
set /p codename=CodeName (%DEFAULT_CODENAME%): 
set /p majorversion=MajorVersion (%DEFAULT_MAJORVERSION%): 
if "%majorversion%"=="" set "majorversion=%DEFAULT_MAJORVERSION%"
if "%codename%"=="" set "codename=%DEFAULT_CODENAME%"

set "targetDir=Builds\%majorversion%\%codename%\%version%"

mkdir "%targetDir%" 2>nul

xcopy "publish\*.*" "%targetDir%\" /s /e /y /i

del "%targetDir%\PvzLauncher.deps.json"
del "%targetDir%\PvzLauncher.pdb"
del "%targetDir%\bin\PvzLauncherRemake.deps.json"
del "%targetDir%\bin\PvzLauncherRemake.pdb"
del "%targetDir%\bin\UpdateService.deps.json"
del "%targetDir%\bin\UpdateService.pdb"

pause