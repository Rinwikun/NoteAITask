#!/usr/bin/env bash
set -e

REPO="Rinwikun/NoteAITask"
INSTALL_DIR="/usr/local/bin"
BINARY_NAME="NoteAITask-Linux-x64"
APP_NAME="noteaitask"

echo " Installing NoteAI Task Terminal for Linux..."

# 1. Unduh Binary Terbaru dari GitHub Releases
DOWNLOAD_URL="https://github.com/${REPO}/releases/latest/download/${BINARY_NAME}"

echo " Downloading latest release from GitHub..."
curl -fsSL "$DOWNLOAD_URL" -o "/tmp/${APP_NAME}"

# 2. Set Izin Executable & Pindahkan ke System Path
echo " Installing binary to ${INSTALL_DIR}/${APP_NAME}..."
chmod +x "/tmp/${APP_NAME}"
sudo mv "/tmp/${APP_NAME}" "${INSTALL_DIR}/${APP_NAME}"

# 3. Buat Desktop Entry (Agar muncul di App Launcher Linux)
DESKTOP_FILE="/usr/share/applications/noteaitask.desktop"
if [ -d "/usr/share/applications" ]; then
    echo " Registering desktop application entry..."
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

echo " Installation complete! Launch by typing '${APP_NAME}' in terminal or searching in your application menu."