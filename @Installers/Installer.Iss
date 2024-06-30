#define AppVersion "4.2.0"

[Setup]
;-- Main Setup Information
 AppName                          = NetTunnel
 AppVersion                       = {#AppVersion}
 AppVerName                       = NetTunnel {#AppVersion}
 AppCopyright                     = Copyright © 1995-2024 NetworkDLS.
 DefaultDirName                   = {commonpf}\NetworkDLS\NetTunnel
 DefaultGroupName                 = NetworkDLS\NetTunnel
 UninstallDisplayIcon             = {app}\NetTunnel.UI.exe
 SetupIconFile                    = "..\Images\Multi.ico"
 PrivilegesRequired               = admin
 Uninstallable                    = Yes
 MinVersion                       = 0.0,7.0
 Compression                      = bZIP/9
 ChangesAssociations              = Yes
 OutputBaseFilename               = NetTunnel {#AppVersion}
 ArchitecturesInstallIn64BitMode  = x64
 AppPublisher                     = NetworkDLS
 AppPublisherURL                  = http://www.NetworkDLS.com/
 AppUpdatesURL                    = http://www.NetworkDLS.com/

[Components]
 Name: Base;            Description: "Base Install";       Types: full compact custom;  Flags: Fixed;
 Name: Base\Management; Description: "Management Console"; Types: full compact custom;
 Name: Base\Service;    Description: "Tunnel Service";     Types: full compact custom;


[Files]
 Source: "..\NetTunnel.Service\bin\Release\net8.0\runtimes\*.*"; DestDir: "{app}\runtimes";  Flags: IgnoreVersion recursesubdirs;  Components: Base\Management;
 Source: "..\NetTunnel.Service\bin\Release\net8.0\*.exe";        DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Management;
 Source: "..\NetTunnel.Service\bin\Release\net8.0\*.dll";        DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Management;
 Source: "..\NetTunnel.Service\bin\Release\net8.0\*.json";       DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Management;

 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.exe";     DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Service;
 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.dll";     DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Service;
 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.json";    DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Service;
  
[Icons]
 Name: "{commondesktop}\NetTunnel Manager"; Filename: "{app}\NetTunnel.UI.exe";                                                    Components: Base\Management;
 Name: "{group}\NetTunnel Manager";         Filename: "{app}\NetTunnel.UI.exe";                                                    Components: Base\Management;

[Run]
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "install"; Flags: runhidden; StatusMsg: "Installing service...";             Components: Base\Service;
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "start"; Flags: runhidden; StatusMsg: "Starting service...";                 Components: Base\Service;
 Filename: "{app}\NetTunnel.UI.exe"; Description: "Run NetTunnel Manager now?"; Flags: postinstall nowait skipifsilent shellexec;  Components: Base\Management;

[UninstallRun]
 Filename: "{app}\NetTunnel.Service.exe"; Parameters: "uninstall"; Flags: runhidden; StatusMsg: "Installing service..."; RunOnceId: "ServiceRemoval"; Components: Base\Service;
 

