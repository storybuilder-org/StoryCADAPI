@echo off
rem Builds the Outliner sample's Windows (WinUI) target.
dotnet build "%~dp0..\samples\Outliner\Outliner\Outliner.csproj" -f net10.0-windows10.0.22621 -p:Platform=x64 -c Debug
exit /b %ERRORLEVEL%
