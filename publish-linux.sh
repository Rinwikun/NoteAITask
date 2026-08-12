#!/usr/bin/env bash
set -e

echo -e "\e[36m🚀 Starting Linux Native Release Pipeline for NoteAI Task Terminal v3.2.2-beta...\e[0m"

PUBLISH_DIR="NoteAITask/bin/Release/net10.0/linux-x64/publish"
OBFUSCAR_XML="obfuscar.xml"

if [ ! -f "$OBFUSCAR_XML" ]; then
    OBFUSCAR_XML="NoteAITask/obfuscar.xml"
fi

# Step 1: Clean & Build Loose Assemblies
echo -e "\e[33m📦 Step 1: Compiling Loose Assemblies for Linux (linux-x64)...\e[0m"
dotnet publish NoteAITask/NoteAITask.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=false /p:PublishReadyToRun=false

# Step 2: Run Obfuscar Protection
echo -e "\e[33m🛡️ Step 2: Running Obfuscar Protection...\e[0m"
# Menggunakan mono/obfuscar.console sesuai environment Linux
if command -v obfuscar.console &> /dev/null; then
    (cd "$PUBLISH_DIR" && obfuscar.console "$PWD/../../../../$OBFUSCAR_XML")
else
    echo -e "\e[31m❌ Error: obfuscar.console not found in PATH. Install via 'dotnet tool install --global Obfuscar.GlobalTool'\e[0m"
    exit 1
fi

# Step 3: Replace Assembly with Obfuscated Version
echo -e "\e[33m🔄 Step 3: Applying Protected Assemblies...\e[0m"
if [ -f "$PUBLISH_DIR/protected/NoteAITask.dll" ]; then
    cp -f "$PUBLISH_DIR/protected/NoteAITask.dll" "$PUBLISH_DIR/NoteAITask.dll"
    rm -rf "$PUBLISH_DIR/protected"
fi

# Step 4: Final Single-File Bundle for Linux
echo -e "\e[33m⚡ Step 4: Bundling into Protected Single-File Linux Binary...\e[0m"
dotnet publish NoteAITask/NoteAITask.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=false --no-restore

# Make Binary Executable
chmod +x "$PUBLISH_DIR/NoteAITask"

echo -e "\e[32m✅ SUCCESS! Protected Linux Release v3.2.2-beta ready in:\e[0m"
echo -e "\e[36m$PUBLISH_DIR/NoteAITask\e[0m"