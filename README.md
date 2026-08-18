<p align="center">
  <img src="https://github.com/Rinwikun/NoteAITask/blob/main/NoteAITask/Assets/logo/logo-app.png" alt="NoteAI Task Terminal Logo App" width="128" />
</p>

<h1 align="center">NoteAI Task Terminal</h1>

[![License: Proprietary](https://img.shields.io/badge/License-Proprietary-red.svg)](https://github.com/Rinwikun/NoteAITask/blob/main/LICENSE)
[![NET Target](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![AvaloniaUI](https://img.shields.io/badge/UI-AvaloniaUI-purple?logo=avalonia)](https://avaloniaui.net/)
[![Ollama](https://img.shields.io/badge/AI%20Engine-Ollama-black?logo=ollama&logoColor=white)](https://ollama.com/)
[![Release Stage](https://img.shields.io/badge/Release-v3.2.3--beta-brightgreen)]()

> ⚠️ **Language Notice:** Currently, this application interface and logging output are exclusively in **Indonesian (Bahasa Indonesia)**. Multi-language (i18n / English) support is planned for future updates.

**NoteAI Task Terminal** is a smart, desktop-based cross-platform application (Windows, Arch Linux/Ubuntu) integrated with a **Local AI Engine (Ollama)** and a **Deterministic Physical Terminal Agent**.

It seamlessly translates natural language instructions into concrete filesystem structure manifests and terminal commands with zero path leakage.

---

| OS | Successful and Unsuccessful Install |
| :--- | :---: |
| Windows | Successful |
| Linux | Successful |
| MacOS | Unsuccessful |
---

## 🚦 Module Development Progress Tracker

Below is the status matrix of the feature development roadmap for **NoteAI Task Terminal**:

| Feature / Module | Development Status | Stability / Test Grade | Score | Notes |
| :--- | :---: | :---: | :--- | :--- |
| **📝 Note Dual-View** | 🟡 In Progress | 🟢 Stable | 90% |Split-View (3 Columns) & Explorer View options |
| **💻 Note Terminal** | 🟢 Complete | 🟡 Unstable | 50% | Direct execution for PowerShell, CMD, Bash/Zsh |
| **🤖 Note AI Terminal Agent** | 🟡 In Progress | 🟡 Unstable | 25% | Dual-Intent Engine, Manifest Parser & Path Resolver |
| **⚙️ Dynamic AI Settings** | 🟡 In Progress | 🟢 Stable | 7% | Auto-detect Ollama models & Editable System Prompt |
| **🔒 Enterprise Code Protection**| 🟢 Complete | 🟢 Stable | 99% | Obfuscar, Single-File AOT & Symbol Stripping |
| **🌐 Multi-Language (i18n)** | 🔴 Planned | ⚪ Not Started | 0% | English translation planned for future releases |

Development Status: 🟢 Complete, 🟡 In Progress, 🔴 Planned\
Stability / Test Grade: 🟢 Stable, 🟡 Unstable, ⚪ Not Started 

---

## ✨ Key Features

* 📝 **Note Dual-View:** Choose between Split-View (3 Columns) or Explorer View (Hierarchical List).
* 💻 **Note Terminal:** Native shell executor for direct PowerShell, CMD, and Bash/Zsh commands.
* 🤖 **Note AI Terminal Agent:**
  * **Dual-Intent Engine:** Automatically classifies queries between *Read/Query* (system checks) and *Write/Mutation* (directory/file creation).
  * **Target-Root Preservation:** Ensures path creation is strictly performed at absolute target paths (e.g., `D:\`, `/tmp/`).
  * **Physical Path Validator:** Verifies actual file/folder creation directly on the local hard disk to prevent false positives.
  * **Execution Duration Timer:** Real-time execution stopwatch for AI generation and terminal processes.
* ⚙️ **Dynamic AI Configuration:** Auto-detects local Ollama models (`qwen2.5:coder`, `llama3`, etc.) and provides a standalone System Prompt Template editor.


| 🤖 AI Terminal Agent Demo | 📝 Note Dual-View Explorer |
| :---: | :---: |
| <video src="https://github.com/user-attachments/assets/e0830efe-49d3-4cdb-9f49-25aa34add25e" controls="controls" style="max-width: 100%; max-height: 300px;">AI Agent Demo</video> | <video src="https://github.com/user-attachments/assets/5c3ecd4c-848d-4b70-b176-048dfb5ce7e9" controls="controls" style="max-width: 100%; max-height: 300px;">Dual View Demo</video> |

---

## 🛠️ Tech Stack & Architecture

* **UI Framework:** Avalonia UI (Cross-Platform C# / XAML)
* **Design Pattern:** MVVM (CommunityToolkit.Mvvm)
* **Local AI Integration:** Ollama REST API Endpoint (`/api/generate` & `/api/tags`)
* **Shell Executor:** Dynamic Process Adapter (PowerShell / Windows CMD / Linux Bash / macOS Zsh)
* **Protection Layer:** Single-File Native Deployment, Symbol Stripping, and Obfuscar Code Scrambling

### ⚙️ Local AI Engine & System Prompt Configuration
![Settings View Screenshot](https://github.com/Rinwikun/NoteAITask/blob/main/NoteAITask/Assets/images/settings.png)

---

## Image and Video assets are sourced from the official
<details>
  <summary>🖼️ Click to view the image</summary>

  ### image 1
  ![Note](https://github.com/Rinwikun/NoteAITask/blob/main/NoteAITask/Assets/images/note.png)

  ### image 2
  ![Note Terminal](https://github.com/Rinwikun/NoteAITask/blob/main/NoteAITask/Assets/images/note-terminal.png)

  ### image 3
  ![Note AI Terminal](https://github.com/Rinwikun/NoteAITask/blob/main/NoteAITask/Assets/images/note-ai-terminal.png)	
</details>
<details>
  <summary>🎬 Click to view application video demos</summary>

  <br />

  ### 📝 Demo Note
  <video src="https://github.com/user-attachments/assets/5c3ecd4c-848d-4b70-b176-048dfb5ce7e9" controls="controls" style="max-width: 100%;">
  </video>

  ### 💻 Demo Note Terminal 
  <video src="https://github.com/user-attachments/assets/0df8055c-4d47-43ae-8535-d8ab4af3e8bb" controls="controls" style="max-width: 100%;">
  </video>

  ### 🤖 Demo Note AI Terminal
  <video src="https://github.com/user-attachments/assets/e0830efe-49d3-4cdb-9f49-25aa34add25e" controls="controls" style="max-width: 100%;">
  </video>
</details>

---

## 🚀 How to Run the Project

### Prerequisites
1. Download & Install [.NET 10.0 SDK](https://dotnet.microsoft.com/) *(Developers only; end-users running pre-compiled binary releases do not need the SDK)*.
2. Install and run [Ollama](https://ollama.ai/) on your machine.
3. Pull your preferred coding model:
   ```bash
   ollama pull qwen2.5-coder:7b
   ```
### Install NoteAITask on **Windows**

```powershell
# Quick Install using PowerShell (irm)
irm https://raw.githubusercontent.com/Rinwikun/NoteAITask/main/scripts/install.ps1 | iex

# Manual Installation
# Download the latest NoteAITask-Windows-x64.exe binary directly from GitHub Releases:
# https://github.com/Rinwikun/NoteAITask/releases
```
### Install NoteAITask in **Linux**
    ```bash
    # Install using One-Liner Shell Script (curl)
    curl -fsSL https://raw.githubusercontent.com/Rinwikun/NoteAITask/main/scripts/install.sh | sh

    # Run Application
    noteaitask
---

### 🔒 Security & Protection Architecture
#### This application is built using enterprise-grade protection mechanisms:
- **Single-File Executable**: Embedded runtime dependencies; no external framework installation required.
- **Metadata & Symbol Stripping**: Suppresses debug symbol generation to prevent internal source path leaks.
- **Obfuscation Engine**: Integrated Obfuscar pipeline to scramble class, property, and method signatures against decompilers.
- **AOT Compilation**: Ahead-of-Time compilation for native performance and reduced runtime reflection.
- **Local AI Model Enforcement**: By default, all AI model interactions are strictly local via Ollama; no external API calls are made. *(Optional cloud AI provider support via user-supplied API key is planned — see roadmap.)*
- **Path Leakage Prevention**: The AI agent is designed to resolve and validate paths without exposing sensitive directory structures.
- **Execution Sandbox**: All terminal commands are executed in a controlled environment to prevent unauthorized system modifications.

---

## 💬 Feedback, Reviews & Community Testing
### 📌 Solo Developer Note
This application is independently developed by a single developer working on a mid-range laptop. Due to hardware limitations and restricted test environments (especially for macOS and various Linux distributions), comprehensive cross-platform testing is quite challenging.

### 🤝 How You Can Help
We highly value your feedback! If you test or use NoteAI Task Terminal, please consider:
- 🐛 Reporting Bugs: Opening an Issue to report bugs or cross-platform inconsistencies.
- 💡 Feature Requests & Reviews: Leaving your review, suggestions, or feature requests in the Discussions tab.
- 🧪 Testing Log Submissions: Sharing your execution logs or testing results on different OS environments (Linux distributions, macOS, Windows).

## 🤝 Contributing
This repository accepts pull requests. By submitting a contribution, you
agree to the terms in [LICENSE §4 (Contributions)](https://github.com/Rinwikun/NoteAITask/blob/main/LICENSE).
See `CONTRIBUTING.md` for setup and PR guidelines.

## 👤 Author & Credits
Created with ❤️ by Rinwikun
Source code distributed under a [Proprietary Source-Available License](https://github.com/Rinwikun/NoteAITask/blob/main/LICENSE).
End-user application usage governed by the [EULA](https://github.com/Rinwikun/NoteAITask/blob/main/EULA.md).


## ☕ Support the Project
[![Trakteer](https://img.shields.io/badge/Trakteer-Traktir%20Coffee-red?style=for-the-badge&logo=coffee&logoColor=white)](https://trakteer.id/erwin%20wijaya)
[![Saweria](https://img.shields.io/badge/Saweria-Support%20Me-orange?style=for-the-badge&logo=heart&logoColor=white)](https://saweria.co/Rinwikun23)
[![Paypal](https://img.shields.io/badge/Paypal-Support%20Me-orange?style=for-the-badge&logo=heart&logoColor=white)](https://paypal.me/Rinwikun)