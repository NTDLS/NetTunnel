@echo off
set path=%PATH%;C:\Program Files\7-Zip;
set path=C:\Program Files (x86)\Inno Setup 6\;%path%

rd Publish /q /s
rd Output /q /s

md Publish
md Output

REM Compile service for Windows.x64.
dotnet publish ..\NetTunnel.Service -c Release -o publish\Windows.x64\NetTunnel.Service --runtime win-x64 --self-contained false
del Publish\Windows.x64\NetTunnel.Service\*.pdb
iscc ./InnoScripts/NetTunnel.Service.Windows.x64.Iss

REM Compile UI for Windows.x64.
dotnet publish ..\NetTunnel.UI -c Release -o publish\Windows.x64\NetTunnel.UI --runtime win-x64 --self-contained false
del Publish\Windows.x64\NetTunnel.UI\*.pdb
iscc ./InnoScripts/NetTunnel.UI.Windows.x64.Iss


REM Compile service for Windows.x64 (full).
dotnet publish ..\NetTunnel.Service -c Release -o publish\Windows.x64\Full --runtime win-x64 --self-contained false
REM Compile UI for Windows.x64 (full).
dotnet publish ..\NetTunnel.UI -c Release -o publish\Windows.x64\Full --runtime win-x64 --self-contained false
del Publish\Windows.x64\Full\*.pdb
iscc ./InnoScripts/NetTunnel.Full.Windows.x64.Iss

REM Compile service for Linux.x64.
dotnet publish ..\NetTunnel.Service -c Release -o publish\Linux.x64\NetTunnel.Service --runtime linux-x64 --self-contained true
del Publish\Linux.x64\NetTunnel.Service\*.pdb

REM Compile service for Linux.Arm64.
dotnet publish ..\NetTunnel.Service -c Release -o publish\Linux.Arm64\NetTunnel.Service --runtime linux-arm64 --self-contained true
del Publish\Linux.Arm64\NetTunnel.Service\*.pdb

7z.exe a -tzip -r -mx9 ".\Output\NetTunnel.Service.Linux.Arm64.zip" ".\publish\Linux.Arm64\NetTunnel.Service\*.*"
7z.exe a -tzip -r -mx9 ".\Output\NetTunnel.Service.Linux.x64.zip" ".\publish\Linux.x64\NetTunnel.Service\*.*"

rd publish /q /s

pause
