$ErrorActionPreference = "Stop"

$repo = "Rinwikun/NoteAITask"
$binaryName = "NoteAITask-Windows-x64.exe"
$installPath = "$env:LOCALAPPDATA\NoteAITask"
$exePath = "$installPath\NoteAITask.exe"

Write-Host "🚀 Installing NoteAI Task Terminal for Windows..." -ForegroundColor Cyan

if (-not (Test-Path $installPath)) {
    New-Item -ItemType Directory -Path $installPath -Force | Out-Null
}

$downloadUrl = "https://github.com/$repo/releases/latest/download/$binaryName"
Write-Host "📦 Downloading latest release..." -ForegroundColor Yellow
Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath

# Tambahkan ke User PATH jika belum terdaftar
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$installPath*") {
    Write-Host "⚙️ Adding $installPath to User PATH..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$installPath", "User")
}

# Buat Desktop Shortcut
$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut("$env:USERPROFILE\Desktop\NoteAI Task Terminal.lnk")
$shortcut.TargetPath = $exePath
$shortcut.Save()

Write-Host "✅ Installation complete! You can run 'NoteAITask' from PowerShell or click the Desktop Shortcut." -ForegroundColor Green