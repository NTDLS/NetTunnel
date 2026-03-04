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
REM Compile UI for Windows.x64.
dotnet publish ..\NetTunnel.UI -c Release -o publish\Windows.x64\NetTunnel.UI --runtime win-x64 --self-contained false
del Publish\Windows.x64\NetTunnel.UI\*.pdb
REM Compile service for Linux.x64.
dotnet publish ..\NetTunnel.Service -c Release -o publish\Linux.x64\NetTunnel.Service --runtime linux-x64 --self-contained false
del Publish\Linux.x64\NetTunnel.Service\*.pdb

7z.exe a -tzip -r -mx9 ".\Output\NetTunnel.Service.Linux.x64.zip" ".\publish\Linux.x64\NetTunnel.Service\*.*"

iscc ./InnoScripts/NetTunnel.Full.Windows.x64.Iss
iscc ./InnoScripts/NetTunnel.Service.Windows.x64.Iss
iscc ./InnoScripts/NetTunnel.UI.Windows.x64.Iss

rd publish /q /s

pause
