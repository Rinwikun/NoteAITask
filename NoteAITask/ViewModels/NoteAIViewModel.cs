using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Models;
using NoteAITask.Services;

namespace NoteAITask.ViewModels;


public partial class NoteAIViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
    };
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(60) };
    private readonly AppSettingsService _settingsService = new();

    // UI States
    [ObservableProperty]
    private int _selectedOutputMode = 0;

    [ObservableProperty]
    private int _selectedShellType = 0;

    [ObservableProperty]
    private string _userPrompt = string.Empty;

    [ObservableProperty]
    private string _terminalLog = string.Empty;

    [ObservableProperty]
    private bool _isExecuting = false;

    [ObservableProperty]
    private string _aiStatusText = "Ollama Agent: Standby";

    [ObservableProperty]
    private string _detectedEnvironmentInfo = string.Empty;

    [ObservableProperty]
    private string _executionDurationText = string.Empty;

    public NoteAIViewModel()
    {
        DetectEnvironment();
        TerminalLog = $"🤖 AI Agent Dual-Intent Engine (Read Query & Write Manifest).\n📍 Environment: {DetectedEnvironmentInfo}\n----------------------------------------\n";
    }

    private void DetectEnvironment()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            DetectedEnvironmentInfo = "Windows (PowerShell / CMD)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            DetectedEnvironmentInfo = "Linux (Bash)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            DetectedEnvironmentInfo = "macOS (Zsh)";
        else
            DetectedEnvironmentInfo = "Unknown OS";
    }

    [RelayCommand]
    private async Task ExecuteAIPromptAsync()
    {
        if (string.IsNullOrWhiteSpace(UserPrompt)) return;

        IsExecuting = true;
        ExecutionDurationText = "⏱️ Memproses...";
        var stopwatch = Stopwatch.StartNew();

        AiStatusText = "🧠 AI sedang mengklasifikasi Intent (Read vs Write)...";
        AppendLog($"\n[USER REQUEST]:\n{UserPrompt}");

        try
        {
            var settings = _settingsService.LoadSettings();

            // 1. SANITASI URL: Hapus trailing slash agar tidak terjadi double-slash (//api/generate)
            string rawOllamaUrl = string.IsNullOrWhiteSpace(settings.OllamaUrl) ? "http://localhost:11434" : settings.OllamaUrl.Trim();
            string ollamaUrl = rawOllamaUrl.TrimEnd('/');
            string model = string.IsNullOrWhiteSpace(settings.SelectedModel) ? "qwen2.5:coder" : settings.SelectedModel.Trim();

            (string shellExe, string shellArgFormat, string shellName) = GetActiveShellConfig();
            string extractedTargetRoot = ExtractTargetRootPath(UserPrompt);

            // AMBIL TEMPLATE PROMPT DARI SETTINGS LOKAL
            string rawPromptTemplate = settings.SystemPromptTemplate;

            // INJEKSI VARIABEL DINAMIS KE PROMPT
            string systemPrompt = rawPromptTemplate
                .Replace("{OS_DESCRIPTION}", RuntimeInformation.OSDescription)
                .Replace("{SHELL_NAME}", shellName)
                .Replace("{TARGET_ROOT}", extractedTargetRoot.Replace("\\", "\\\\"));

            var requestBody = new OllamaGenerateRequest
            {
                Model = model,
                System = systemPrompt,
                Prompt = UserPrompt,
                Stream = false
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            string requestEndpoint = $"{ollamaUrl}/api/generate";
            AppendLog($"[OLLAMA ENDPOINT]: {requestEndpoint} (Model: {model})");

            var response = await _httpClient.PostAsync(requestEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                string rawJsonManifest = doc.RootElement.GetProperty("response").GetString()?.Trim() ?? "";

                rawJsonManifest = rawJsonManifest.Replace("```json", "").Replace("```", "").Trim();

                AppendLog($"[JSON MANIFEST GENERATED]:\n{rawJsonManifest}\n");

                AgentManifestPlan? manifestPlan = null;
                try
                {
                    manifestPlan = JsonSerializer.Deserialize<AgentManifestPlan>(rawJsonManifest, _jsonOptions);
                }
                catch (Exception pEx)
                {
                    AppendLog($"🔴 Fail to Parse JSON Manifest: {pEx.Message}");
                    IsExecuting = false;
                    return;
                }

                if (manifestPlan == null)
                {
                    AppendLog("🔴 Manifest JSON Null.");
                    IsExecuting = false;
                    return;
                }

                // ==================================================
                // CABANG 1: OPERASI BACA / QUERY (READ ONLY)
                // ==================================================
                if (manifestPlan.ActionType?.ToUpper() == "READ")
                {
                    string readCmd = manifestPlan.ReadCommand;
                    if (string.IsNullOrWhiteSpace(readCmd))
                    {
                        readCmd = shellName.Contains("PowerShell") ? "Get-ChildItem" : "ls -la";
                    }

                    AppendLog($"[EXECUTING READ COMMAND]: {readCmd}");
                    AiStatusText = $"⚡ Membaca data sistem via {shellName}...";

                    string rawReadOutput = RunShellCommand(shellExe, shellArgFormat, readCmd);
                    string formattedResult = ProcessOutputMode(rawReadOutput, SelectedOutputMode);

                    AppendLog($"[TERMINAL SYSTEM OUTPUT]:\n{formattedResult}");
                    AiStatusText = "🟢 Pembacaan Selesai!";
                }
                // ==================================================
                // CABANG 2: OPERASI BUAT FILE/FOLDER (WRITE)
                // ==================================================
                else
                {
                    if (manifestPlan.Nodes == null || manifestPlan.Nodes.Count == 0)
                    {
                        AppendLog("🔴 Operation Write tetapi tidak ada node yang didaftarkan.");
                        IsExecuting = false;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(manifestPlan.TargetRoot) || manifestPlan.TargetRoot == "DEFAULT")
                    {
                        manifestPlan.TargetRoot = !string.IsNullOrWhiteSpace(extractedTargetRoot) ? extractedTargetRoot : Directory.GetCurrentDirectory();
                    }

                    string absoluteRootDirectory = Path.Combine(manifestPlan.TargetRoot, manifestPlan.RootName);
                    AppendLog($"[TARGET RESOLVED ABSOLUTE PATH]: {absoluteRootDirectory}");

                    string generatedCommand = BuildShellCommandFromManifest(manifestPlan, absoluteRootDirectory, shellName);
                    AppendLog($"[GENERATED ADAPTER COMMAND]:\n{generatedCommand}\n");

                    AiStatusText = $"⚡ Mengeksekusi pembuatan folder/file di {shellName}...";
                    string rawWriteResult = RunShellCommand(shellExe, shellArgFormat, generatedCommand);

                    ValidatePhysicalManifest(manifestPlan, absoluteRootDirectory);

                    string formattedResult = ProcessOutputMode(rawWriteResult, SelectedOutputMode);
                    AppendLog($"[TERMINAL RAW OUTPUT]:\n{formattedResult}");

                    AiStatusText = "🟢 Eksekusi & Validation Selesai!";
                }
            }
            else
            {
                // LOG DIAGNOSTIK KETIKA OLLAMA MERESPON ERROR STATUS CODE
                string errDetails = await response.Content.ReadAsStringAsync();
                AppendLog($"🔴 Error Ollama HTTP Status: {response.StatusCode}\n[DETAILS]: {errDetails}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            AppendLog($"🔴 Connection Error: Gagal terhubung ke Ollama. Pastikan Ollama aktif di URL target.\n[DETAIL EXCEPTION]: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            AppendLog($"🔴 Exception: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            IsExecuting = false;

            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            ExecutionDurationText = $"⏱️ Waktu Selesai: {totalSeconds:F2} detik";
            AppendLog($"⏱️ [TOTAL EXECUTION TIME]: {totalSeconds:F2} detik\n");
        }
    }

    private string ExtractTargetRootPath(string prompt)
    {
        var matchWin = Regex.Match(prompt, @"([a-zA-Z]:\\[^ \r\n\t]*|[a-zA-Z]:)");
        if (matchWin.Success)
        {
            string path = matchWin.Value.Trim();
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        var matchUnix = Regex.Match(prompt, @"(/[a-zA-Z0-9_\-\.]+)+");
        if (matchUnix.Success)
        {
            return matchUnix.Value.Trim();
        }

        return string.Empty;
    }

    private string BuildShellCommandFromManifest(AgentManifestPlan plan, string absoluteRootDirectory, string shellName)
    {
        var sb = new StringBuilder();

        bool isPowerShell = shellName.Contains("PowerShell");
        bool isCMD = shellName.Contains("CMD");

        foreach (var node in plan.Nodes)
        {
            string fullPath = Path.Combine(absoluteRootDirectory, node.RelativePath.Replace("/", "\\"));

            if (node.Type.ToLower() == "directory")
            {
                if (isPowerShell)
                    sb.Append($"New-Item -Path '{fullPath}' -ItemType Directory -Force | Out-Null; ");
                else if (isCMD)
                    sb.Append($"if not exist \"{fullPath}\" mkdir \"{fullPath}\" && ");
                else
                    sb.Append($"mkdir -p \"{fullPath}\" && ");
            }
            else
            {
                string? dirName = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dirName))
                {
                    if (isPowerShell)
                        sb.Append($"New-Item -Path '{dirName}' -ItemType Directory -Force | Out-Null; ");
                    else if (isCMD)
                        sb.Append($"if not exist \"{dirName}\" mkdir \"{dirName}\" && ");
                    else
                        sb.Append($"mkdir -p \"{dirName}\" && ");
                }

                if (isPowerShell)
                    sb.Append($"New-Item -Path '{fullPath}' -ItemType File -Force | Out-Null; ");
                else if (isCMD)
                    sb.Append($"type nul > \"{fullPath}\" && ");
                else
                    sb.Append($"touch \"{fullPath}\" && ");
            }
        }

        string resultCmd = sb.ToString().Trim();
        if (resultCmd.EndsWith(";")) resultCmd = resultCmd[..^1];
        if (resultCmd.EndsWith("&&")) resultCmd = resultCmd[..^2];

        return resultCmd;
    }

    private void ValidatePhysicalManifest(AgentManifestPlan plan, string absoluteRootDirectory)
    {
        AppendLog("\n🔍 --- [STRICT PHYSICAL ABSOLUTE PATH VALIDATION] ---");
        AppendLog($"EXPECTED PHYSICAL ROOT: {absoluteRootDirectory}");

        int totalNodes = plan.Nodes.Count;
        int verifiedCount = 0;

        foreach (var node in plan.Nodes)
        {
            string absolutePath = Path.Combine(absoluteRootDirectory, node.RelativePath.Replace("/", "\\"));
            bool exists = node.Type.ToLower() == "directory"
                          ? Directory.Exists(absolutePath)
                          : File.Exists(absolutePath);

            if (exists)
            {
                AppendLog($"  ✅ [VERIFIED]: {absolutePath} ({node.Type.ToUpper()})");
                verifiedCount++;
            }
            else
            {
                AppendLog($"  ❌ [TARGET ROOT MISMATCH / MISSING]: {node.RelativePath} -> NOT FOUND AT: {absolutePath}");
            }
        }

        if (verifiedCount == totalNodes)
        {
            AppendLog($"\nRESULT: 100% PERFECT MATCH AT TARGET ROOT ({verifiedCount}/{totalNodes} nodes verified at {absoluteRootDirectory}).\n");
        }
        else
        {
            AppendLog($"\nRESULT: 🔴 TARGET ROOT MISMATCH OR INCOMPLETE EXECUTION ({verifiedCount}/{totalNodes} nodes found at target path).\n");
        }
    }

    private (string Exe, string ArgFormat, string Name) GetActiveShellConfig()
    {
        if (SelectedShellType == 1) return ("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", "PowerShell");
        if (SelectedShellType == 2) return ("cmd.exe", "/c \"{0}\"", "Windows CMD");
        if (SelectedShellType == 3) return ("/bin/bash", "-c \"{0}\"", "Linux/macOS Bash");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return ("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", "PowerShell (Auto-Detected)");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return ("/bin/zsh", "-c \"{0}\"", "macOS Zsh (Auto-Detected)");
        else
            return ("/bin/bash", "-c \"{0}\"", "Linux Bash (Auto-Detected)");
    }

    private string RunShellCommand(string shellExe, string argFormat, string command)
    {
        try
        {
            string sanitizedCommand = command.Trim();

            // MENCEGAH UNTERMINATED QUOTE: jika command diakhiri backslash tunggal (seperti D:\), ganti menjadi double backslash
            if (sanitizedCommand.EndsWith("\\") && !sanitizedCommand.EndsWith("\\\\"))
            {
                sanitizedCommand += "\\";
            }

            sanitizedCommand = sanitizedCommand.Replace("\"", "\\\"");
            string formattedArgs = string.Format(argFormat, sanitizedCommand);

            var processInfo = new ProcessStartInfo
            {
                FileName = shellExe,
                Arguments = formattedArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(10000);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return $"STDERR:\n{error}\nSTDOUT:\n{output}";
            }

            return string.IsNullOrWhiteSpace(output) ? "(Perintah berhasil dieksekusi tanpa output teks)" : output;
        }
        catch (Exception ex)
        {
            return $"EXECUTION ERROR: {ex.Message}";
        }
    }

    private string ProcessOutputMode(string rawOutput, int mode)
    {
        if (string.IsNullOrWhiteSpace(rawOutput)) return "(Output Kosong)";

        return mode switch
        {
            1 => FilterSetengahName(rawOutput),
            2 => FilterSetengahLength(rawOutput),
            _ => rawOutput
        };
    }

    private string FilterSetengahName(string raw)
    {
        string[] lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        sb.AppendLine("--- [MODE: SETENGAH NAME] ---");
        foreach (var line in lines)
        {
            string trimmed = line.Trim();
            if (!trimmed.StartsWith("-") && !trimmed.StartsWith("Directory") && !trimmed.StartsWith("Mode"))
            {
                sb.AppendLine($"📌 {trimmed}");
            }
        }
        return sb.ToString();
    }

    private string FilterSetengahLength(string raw)
    {
        string[] lines = raw.Split('\n');
        var sb = new StringBuilder();
        sb.AppendLine("--- [MODE: SETENGAH LENGTH (MAX 8 BARIS)] ---");
        int limit = Math.Min(lines.Length, 8);
        for (int i = 0; i < limit; i++)
        {
            sb.AppendLine(lines[i]);
        }
        if (lines.Length > 8)
        {
            sb.AppendLine($"\n... [{lines.Length - 8} baris dipangkas]");
        }
        return sb.ToString();
    }

    [RelayCommand]
    private void ClearLog()
    {
        TerminalLog = $"🧹 Terminal log dibersihkan.\n📍 Environment: {DetectedEnvironmentInfo}\n----------------------------------------\n";
        ExecutionDurationText = string.Empty;
    }

    private void AppendLog(string text)
    {
        TerminalLog += $"{text}\n";
    }
}