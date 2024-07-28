[Setup]
AppName=VnResourceTAMHub
AppVersion=1.0
DefaultDirName={pf}\VnResourceTAMHub
DefaultGroupName=VnResourceTAMHub
OutputBaseFilename=VnResourceTAMHub
OutputDir=D:\Downloads\VnResourceTAMHub
SetupIconFile=C:\Users\anhvu\OneDrive\Desktop\TAMHub\VnResourceTAMHub\TAMHubApp\bin\Release\net8.0-windows\publish\Resources\Images\icon.ico
SolidCompression=yes
WizardStyle=modern

ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "C:\Users\anhvu\OneDrive\Desktop\TAMHub\VnResourceTAMHub\TAMHubApp\bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\VnResourceTAMHub"; Filename: "{app}\TAMHubApp.exe"; IconFilename: "{app}\Resources\Images\icon.ico"
Name: "{commondesktop}\VnResourceTAMHub"; Filename: "{app}\TAMHubApp.exe"; IconFilename: "{app}\Resources\Images\icon.ico"

[Dirs]
Name: {app}; Permissions: users-full