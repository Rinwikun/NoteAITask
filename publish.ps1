Write-Host "Starting Release Pipeline for NoteAI Task Terminal v3.2.2-beta..." -ForegroundColor Cyan

$publishDir = "NoteAITask\bin\Release\net10.0\win-x64\publish"

# Cari lokasi obfuscar.xml (root vs subfolder)
$obfuscarXml = "$PSScriptRoot\obfuscar.xml"
if (-not (Test-Path $obfuscarXml)) {
    $obfuscarXml = "$PSScriptRoot\NoteAITask\obfuscar.xml"
}

# Step 1: Clean & Build Loose Assemblies
Write-Host "Step 1: Compiling Loose Assemblies..." -ForegroundColor Yellow
dotnet publish NoteAITask/NoteAITask.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=false /p:PublishReadyToRun=false

# Step 2: Obfuscate Assembly
Write-Host "Step 2: Running Obfuscar Protection using config: $obfuscarXml" -ForegroundColor Yellow
Set-Location -Path $publishDir
obfuscar.console "$obfuscarXml"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Obfuscation failed!" -ForegroundColor Red
    Set-Location -Path $PSScriptRoot
    exit 1
}

# Step 3: Replace with Obfuscated Version
Write-Host "Step 3: Applying Protected Assemblies..." -ForegroundColor Yellow
if (Test-Path ".\protected\NoteAITask.dll") {
    Copy-Item -Path ".\protected\NoteAITask.dll" -Destination ".\NoteAITask.dll" -Force
    Remove-Item -Path ".\protected" -Recurse -Force
}
Set-Location -Path $PSScriptRoot

# Step 4: Final Single-File Bundle
Write-Host "Step 4: Bundling into Protected Single-File Executable..." -ForegroundColor Yellow
dotnet publish NoteAITask/NoteAITask.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=false --no-restore

Write-Host "SUCCESS! Protected Release v3.2.2-beta ready in:" -ForegroundColor Green
Write-Host "$publishDir\NoteAITask.exe" -ForegroundColor Cyan
