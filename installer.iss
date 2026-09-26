; اسکریپت Inno Setup - از پوشه‌ی خروجی بیلد یک نصب‌کننده‌ی واحد (Setup.exe) می‌سازد.
; در CI با ISCC.exe (که روی رانرهای windows-latest از پیش نصب است) کامپایل می‌شود.

#define MyAppName "مدیریت تپه و فاکتور خاش"
#define MyAppVersion "1.0.0"
#define MyAppExeName "TapeKhashDesktop.exe"

[Setup]
AppId={{B6B8B6C1-3E7E-4C7B-9B7A-TAPEKHASH0001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\TapeKhashDesktop
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=TapeKhashDesktop-Setup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
; ویندوز ۷ SP1 به بالا
MinVersion=6.1

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "اجرای برنامه"; Flags: nowait postinstall skipifsilent
