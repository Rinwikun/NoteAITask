using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace NoteAITask.Services;

public class AppSettingsData
{
    public bool IsDefaultViewA { get; set; } = true;
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string SelectedModel { get; set; } = "qwen2.5-coder:7b";
    public bool UseAutoDetectModel { get; set; } = false;

    // CUSTOM SYSTEM PROMPTS (EDITABLE VIA SETTINGS)
    public string SystemPromptTemplate { get; set; } = @"
Kamu adalah World-Class Cross-Platform AI Terminal Agent.
Tugas utama: Analisa intent dari request user, lalu klasifikasikan apakah user ingin MEMBACA/MENGECEK ('READ') atau MEMBUAT/MODIFIKASI ('WRITE').

Target OS: {OS_DESCRIPTION}
Target Shell Executor: {SHELL_NAME}

KONTRAK OUTPUT JSON (STRICT FORMAT):

JIKA USER INGIN MEMBACA / MENGECEK (Contoh: 'cek folder apa aja di C:', 'tampilkan port', 'isi file x'):
{
  ""actionType"": ""READ"",
  ""readCommand"": ""Get-ChildItem -Path D:\\""
}

JIKA USER INGIN MEMBUAT / STRUKTUR DIREKTORI & FILE:
{
  ""actionType"": ""WRITE"",
  ""targetRoot"": ""{TARGET_ROOT}"",
  ""rootName"": ""nama-folder-root"",
  ""nodes"": [
    { ""type"": ""directory"", ""relativePath"": ""src"" },
    { ""type"": ""file"", ""relativePath"": ""src/index.js"" }
  ]
}

ATURAN STRICT:
1. DILARANG MEMBUAT FILE/FOLDER BARU jika intent user HANYA MENANYAKAN/MENGECEK ISI! Gunakan actionType = 'READ'.
2. Pada 'READ', isi 'readCommand' dengan command terminal valid yang sesuai dengan {SHELL_NAME}.
3. HANYA keluarkan JSON mentah tanpa markdown (```json).
";
}

public class AppSettingsService
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
    public AppSettingsService()
    {
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configDir = Path.Combine(appDataFolder, "NoteAITask");

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        _filePath = Path.Combine(configDir, "settings.json");
    }

    public AppSettingsData LoadSettings()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<AppSettingsData>(json);
                if (data != null) return data;
            }
        }
        catch { }

        return new AppSettingsData();
    }

    public void SaveSettings(AppSettingsData settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch { }
    }
}