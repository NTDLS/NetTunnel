#define AppVersion "4.4.0"

[Setup]
;-- Main Setup Information
 AppName                          = NetTunnel
 AppVersion                       = {#AppVersion}
 AppVerName                       = NetTunnel {#AppVersion}
 AppCopyright                     = Copyright © 1995-2026 NetworkDLS.
 DefaultDirName                   = {commonpf}\NetworkDLS\NetTunnel
 DefaultGroupName                 = NetworkDLS\NetTunnel
 UninstallDisplayIcon             = {app}\NetTunnel.Service.exe
 SetupIconFile                    = "..\..\Images\Multi.ico"
 PrivilegesRequired               = admin
 Uninstallable                    = Yes
 MinVersion                       = 0.0,7.0
 Compression                      = bZIP/9
 ChangesAssociations              = Yes
 OutputDir                        = /../Output/
 OutputBaseFilename               = NetTunnel.Service.Windows.x64
 ArchitecturesInstallIn64BitMode  = x64Compatible
 AppPublisher                     = NetworkDLS
 AppPublisherURL                  = http://www.NetworkDLS.com/
 AppUpdatesURL                    = http://www.NetworkDLS.com/

[Components]
 Name: Base;            Description: "Base Install";       Types: full compact custom;  Flags: Fixed;
 Name: Base\Management; Description: "Management Console"; Types: full compact custom;
 Name: Base\Service;    Description: "Tunnel Service";     Types: full compact custom;


[Files]
 Source: "..\Publish\Windows.x64\NetTunnel.Service\*.*";   DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Service;

 [Run]
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "install"; Flags: runhidden; StatusMsg: "Installing service...";             Components: Base\Service;
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "start"; Flags: runhidden; StatusMsg: "Starting service...";                 Components: Base\Service;

[UninstallRun]
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "uninstall"; Flags: runhidden; StatusMsg: "Installing service..."; RunOnceId: "ServiceRemoval"; Components: Base\Service;
 

