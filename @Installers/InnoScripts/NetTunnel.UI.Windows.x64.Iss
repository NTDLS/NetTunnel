#define AppVersion "4.4.0"

[Setup]
 AppName                          = NetTunnel
 AppVersion                       = {#AppVersion}
 AppVerName                       = NetTunnel {#AppVersion}
 AppCopyright                     = Copyright © 1995-2026 NetworkDLS.
 DefaultDirName                   = {commonpf}\NetworkDLS\NetTunnel
 DefaultGroupName                 = NetworkDLS\NetTunnel
 UninstallDisplayIcon             = {app}\NetTunnel.UI.exe
 SetupIconFile                    = "..\..\Images\Multi.ico"
 PrivilegesRequired               = admin
 Uninstallable                    = Yes
 MinVersion                       = 0.0,7.0
 Compression                      = bZIP/9
 ChangesAssociations              = Yes
 OutputDir                        = /../Output/
 OutputBaseFilename               = NetTunnel.UI.Windows.x64
 ArchitecturesInstallIn64BitMode  = x64Compatible
 AppPublisher                     = NetworkDLS
 AppPublisherURL                  = http://www.NetworkDLS.com/
 AppUpdatesURL                    = http://www.NetworkDLS.com/

[Components]
 Name: Base;            Description: "Base Install";       Types: full compact custom;  Flags: Fixed;
 Name: Base\Management; Description: "Management Console"; Types: full compact custom;

[Files]
 Source: "..\Publish\Windows.x64\NetTunnel.UI\*.*";        DestDir: "{app}";           Flags: IgnoreVersion;                 Components: Base\Management;
  
[Icons]
 Name: "{commondesktop}\NetTunnel Manager"; Filename: "{app}\NetTunnel.UI.exe";                                                    Components: Base\Management;
 Name: "{group}\NetTunnel Manager";         Filename: "{app}\NetTunnel.UI.exe";                                                    Components: Base\Management;

[Run]
 Filename: "{app}\NetTunnel.UI.exe"; Description: "Run NetTunnel Manager now?"; Flags: postinstall nowait skipifsilent shellexec;  Components: Base\Management;
