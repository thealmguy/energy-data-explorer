@echo off
setlocal

set REPO_ROOT=%~dp0
set SOLUTION=%REPO_ROOT%energy-data-explorer.sln
if not exist "%SOLUTION%" set SOLUTION=%REPO_ROOT%energy-data-explorer.slnx

if not exist "%SOLUTION%" (
  echo Solution file not found: %REPO_ROOT%energy-data-explorer.sln or %REPO_ROOT%energy-data-explorer.slnx
  exit /b 1
)

dotnet restore "%SOLUTION%" || exit /b 1
dotnet build "%SOLUTION%" -c Release || exit /b 1

echo Build completed successfully.
