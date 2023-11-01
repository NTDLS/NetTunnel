#define AppVersion "0.2.0.0"

[Setup]
;-- Main Setup Information
 AppName                          = NetTunnel
 AppVersion                       = {#AppVersion}
 AppVerName                       = NetTunnel {#AppVersion}
 AppCopyright                     = Copyright © 1995-2023 NetworkDLS.
 DefaultDirName                   = {commonpf}\NetworkDLS\NetTunnel
 DefaultGroupName                 = NetworkDLS\NetTunnel
 UninstallDisplayIcon             = {app}\UI\NetTunnel.UI.exe
 SetupIconFile                    = "..\Images\Multi.ico"
 PrivilegesRequired               = admin
 Uninstallable                    = Yes
 MinVersion                       = 0.0,7.0
 Compression                      = bZIP/9
 ChangesAssociations              = Yes
 OutputBaseFilename               = NetTunnel
 ArchitecturesInstallIn64BitMode  = x64
 AppPublisher                     = NetworkDLS
 AppPublisherURL                  = http://www.NetworkDLS.com/
 AppUpdatesURL                    = http://www.NetworkDLS.com/

[Files]
 Source: "..\NetTunnel.Service\bin\Release\net7.0\runtimes\*.*"; DestDir: "{app}\Service\runtimes";  Flags: IgnoreVersion recursesubdirs;
 Source: "..\NetTunnel.Service\bin\Release\net7.0\*.exe";        DestDir: "{app}\Service";           Flags: IgnoreVersion;
 Source: "..\NetTunnel.Service\bin\Release\net7.0\*.dll";        DestDir: "{app}\Service";           Flags: IgnoreVersion;
 Source: "..\NetTunnel.Service\bin\Release\net7.0\*.json";       DestDir: "{app}\Service";           Flags: IgnoreVersion;

 Source: "..\NetTunnel.UI\bin\Release\net7.0-windows\*.exe";     DestDir: "{app}\UI";                Flags: IgnoreVersion;
 Source: "..\NetTunnel.UI\bin\Release\net7.0-windows\*.dll";     DestDir: "{app}\UI";                Flags: IgnoreVersion;
 Source: "..\NetTunnel.UI\bin\Release\net7.0-windows\*.json";    DestDir: "{app}\UI";                Flags: IgnoreVersion;
  
[Icons]
 Name: "{commondesktop}\NetTunnel Manager";  Filename: "{app}\UI\NetTunnel.UI.exe";

[Run]
 Filename: "{app}\Service\NetTunnel.Service.exe"; Parameters: "install"; Flags: runhidden; StatusMsg: "Installing service..."
 Filename: "{app}\Service\NetTunnel.Service.exe"; Parameters: "start"; Flags: runhidden; StatusMsg: "Starting service..."
 Filename: "{app}\UI\NetTunnel.UI.exe"; Description: "Run NetTunnel Manager now?"; Flags: postinstall nowait skipifsilent shellexec;


[UninstallRun]
 Filename: "{app}\Service\NetTunnel.Service.exe"; Parameters: "uninstall"; Flags: runhidden; StatusMsg: "Installing service..."; RunOnceId: "ServiceRemoval"
 

