$ErrorActionPreference = "Stop"

# Enforce TLS 1.2 Protocol untuk Koneksi GitHub
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$repo = "Rinwikun/NoteAITask"
$binaryName = "NoteAITask-Windows-x64.exe"
$installPath = "$env:LOCALAPPDATA\NoteAITask"
$exePath = "$installPath\NoteAITask.exe"

Write-Host "🚀 Installing NoteAI Task Terminal for Windows..." -ForegroundColor Cyan

# Buat folder instalasi jika belum ada
if (-not (Test-Path $installPath)) {
    New-Item -ItemType Directory -Path $installPath -Force | Out-Null
}

try {
    # 1. Cari URL Download Biner via GitHub API (Mendukung Pre-Release)
    Write-Host "🔍 Resolving latest release binaries from GitHub API..." -ForegroundColor Yellow
    $apiUrl = "https://api.github.com/repos/$repo/releases"
    $webClient = New-Object System.Net.WebClient
    $webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)")
    
    $jsonResponse = $webClient.DownloadString($apiUrl)
    # Parse JSON Release
    $releases = $jsonResponse | ConvertFrom-Json
    
    # Ambil release paling atas (terbaru)
    $latestRelease = $releases[0]
    $asset = $latestRelease.assets | Where-Object { $_.name -eq $binaryName }

    if (-not $asset) {
        Write-Error "Asset '$binaryName' not found in release '$($latestRelease.tag_name)'"
        exit 1
    }

    $downloadUrl = $asset.browser_download_url
    Write-Host "📦 Downloading release ($($latestRelease.tag_name)) from $downloadUrl..." -ForegroundColor Yellow

    # 2. Download File Executable
    $webClient.DownloadFile($downloadUrl, $exePath)
} catch {
    Write-Host "❌ Download failed! Error detail: $_" -ForegroundColor Red
    exit 1
}

# 3. Tambahkan ke User PATH jika belum terdaftar
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$installPath*") {
    Write-Host "⚙️ Adding $installPath to User PATH..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$installPath", "User")
}

# 4. Buat Shortcut di Desktop
$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut("$env:USERPROFILE\Desktop\NoteAI Task Terminal.lnk")
$shortcut.TargetPath = $exePath
$shortcut.Save()

Write-Host "✅ Installation complete! Run 'NoteAITask' from PowerShell or click the Desktop Shortcut." -ForegroundColor Green