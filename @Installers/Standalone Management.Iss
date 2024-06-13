#define AppVersion "2.2.0"

[Setup]
;-- Main Setup Information
 AppName                          = NetTunnel Management
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
 OutputBaseFilename               = NetTunnel Management {#AppVersion}
 ArchitecturesInstallIn64BitMode  = x64
 AppPublisher                     = NetworkDLS
 AppPublisherURL                  = http://www.NetworkDLS.com/
 AppUpdatesURL                    = http://www.NetworkDLS.com/

[Files]
 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.exe";  DestDir: "{app}"; Flags: IgnoreVersion;
 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.dll";  DestDir: "{app}"; Flags: IgnoreVersion;
 Source: "..\NetTunnel.UI\bin\Release\net8.0-windows\*.json"; DestDir: "{app}"; Flags: IgnoreVersion;
  
[Icons]
 Name: "{commondesktop}\NetTunnel Manager"; Filename: "{app}\NetTunnel.UI.exe";
 Name: "{group}\NetTunnel Manager";         Filename: "{app}\NetTunnel.UI.exe";

[Run]
 Filename: "{app}\NetTunnel.UI.exe"; Description: "Run NetTunnel Manager now?"; Flags: postinstall nowait skipifsilent shellexec;
