@echo off
rem Builds the StoryCADCritter sample's Windows (WinUI) target.
dotnet build "%~dp0..\samples\StoryCADCritter\StoryCADCritter.csproj" -f net10.0-windows10.0.22621 -p:Platform=x64 -c Debug
exit /b %ERRORLEVEL%
