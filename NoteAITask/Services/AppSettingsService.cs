using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;

namespace NoteAITask.Services;

public class AppSettingsData
{
    public bool IsDefaultViewA { get; set; } = true;
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string SelectedModel { get; set; } = "qwen2.5-coder:7b";
    public bool UseAutoDetectModel { get; set; } = false;
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
    public string DebugFilePath => _filePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
    public AppSettingsService()
    {
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configDir = Path.Combine(appDataFolder, "NoteAITask");
        Directory.CreateDirectory(configDir);

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        _filePath = Path.Combine(configDir, "settings.json");
    }

    public AppSettingsData LoadSettings()
    {
        const int maxRetries = 3;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (!File.Exists(_filePath)) return new AppSettingsData();

                string json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<AppSettingsData>(json);
                if (data != null) return data;

                break; // json valid tapi null — tidak ada gunanya retry
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                // File kemungkinan sedang di-lock sesaat oleh proses lain (AV/indexer) pasca rename.
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppSettingsService] Load gagal: {ex}");
                break;
            }
        }

        return new AppSettingsData();
    }
    /// <summary>
    /// Menulis settings secara atomik (write-to-temp lalu replace) dan MELEMPAR exception
    /// kalau gagal, supaya caller wajib menangani kegagalan — bukan diam-diam sukses palsu.
    /// </summary>
    public void SaveSettings(AppSettingsData settings)
    {
        string tempFile = _filePath + ".tmp";
        string json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(tempFile, json);

        // Replace atomik: menghindari file settings.json corrupt/setengah-tertulis
        // kalau proses mati di tengah jalan, dan menghindari torn-write kalau ada
        // instance AppSettingsService lain yang baca bersamaan.
        File.Move(tempFile, _filePath, overwrite: true);
    }
}
