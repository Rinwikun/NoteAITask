#!/usr/bin/env bash
set -e

REPO="Rinwikun/NoteAITask"
INSTALL_DIR="/usr/local/bin"
BINARY_NAME="NoteAITask"
APP_NAME="noteaitask"

echo -e "\e[36m🚀 Installing NoteAI Task Terminal for Linux...\e[0m"

# 1. Unduh Binary Teranyar via GitHub API (Mendukung Pre-Release)
echo -e "\e[33m🔍 Fetching latest release info from GitHub API...\e[0m"
DOWNLOAD_URL=$(curl -s "https://api.github.com/repos/${REPO}/releases" | grep "browser_download_url" | grep "/${BINARY_NAME}\"" | head -n 1 | cut -d '"' -f 4)

if [ -z "$DOWNLOAD_URL" ]; then
    echo -e "\e[31m❌ Error: Binary '${BINARY_NAME}' not found in GitHub Releases!\e[0m"
    exit 1
fi

echo -e "\e[33m📦 Downloading binary from: ${DOWNLOAD_URL}\e[0m"
curl -fsSL "$DOWNLOAD_URL" -o "/tmp/${APP_NAME}"

# 2. Set Izin Executable & Pindahkan ke System Path
echo -e "\e[33m⚙️ Installing binary to ${INSTALL_DIR}/${APP_NAME}...\e[0m"
chmod +x "/tmp/${APP_NAME}"
sudo mv "/tmp/${APP_NAME}" "${INSTALL_DIR}/${APP_NAME}"

# 3. Buat Desktop Entry
DESKTOP_FILE="/usr/share/applications/noteaitask.desktop"
if [ -d "/usr/share/applications" ]; then
    echo -e "\e[33m🖥️ Registering desktop application entry...\e[0m"
    sudo bash -c "cat <<EOF > ${DESKTOP_FILE}
[Desktop Entry]
Name=NoteAI Task Terminal
Comment=NoteAI Task Terminal Desktop Application
Exec=${INSTALL_DIR}/${APP_NAME}
Icon=utilities-terminal
Terminal=false
Type=Application
Categories=Utility;Development;
EOF"
fi

echo -e "\e[32m✅ Installation complete! Launch by typing '${APP_NAME}' in terminal or searching in your application menu.\e[0m"