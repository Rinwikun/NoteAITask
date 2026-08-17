using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private readonly string _settingsFilePath;
    public string DebugFilePath => _settingsFilePath;


    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
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

        _settingsFilePath = Path.Combine(configDir, "settings.json");
    }

    public AppSettingsData LoadSettings()
    {
        Debug.WriteLine($"[DEBUG-SETTINGS] ----------------------------------------");
        Debug.WriteLine($"[DEBUG-SETTINGS] [READ] Target File Path: {_settingsFilePath}");
        Console.WriteLine($"[DEBUG-SETTINGS] [READ] Target File Path: {_settingsFilePath}");

        if (!File.Exists(_settingsFilePath))
        {
            Debug.WriteLine($"[DEBUG-SETTINGS] [READ] File 'settings.json' TIDAK DITEMUKAN. Memuat default fallback (IsDefaultViewA = True).");
            Console.WriteLine($"[DEBUG-SETTINGS] [READ] File 'settings.json' TIDAK DITEMUKAN. Memuat default fallback (IsDefaultViewA = True).");
            return new AppSettingsData();
        }
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                Debug.WriteLine($"[DEBUG-SETTINGS] [READ] Raw File Content from Disk:\n{json}");
                Console.WriteLine($"[DEBUG-SETTINGS] [READ] Raw File Content from Disk:\n{json}");

                var result = JsonSerializer.Deserialize<AppSettingsData>(json, _jsonOptions) ?? new AppSettingsData();

                Debug.WriteLine($"[DEBUG-SETTINGS] [READ SUCCESS] Parsed IsDefaultViewA = {result.IsDefaultViewA}");
                Console.WriteLine($"[DEBUG-SETTINGS] [READ SUCCESS] Parsed IsDefaultViewA = {result.IsDefaultViewA}");
                Debug.WriteLine($"[DEBUG-SETTINGS] ----------------------------------------");

                return result;
            }
            catch (IOException ex) when (attempt < maxRetries - 1)
            {
                // File dikunci sementara oleh proses lain (indexer/AV), tunggu 50ms lalu coba lagi
                Debug.WriteLine($"[DEBUG-SETTINGS] [READ RETRY] File locked, retrying attempt {attempt + 1}... Error: {ex.Message}");
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG-SETTINGS] [READ ERROR] Gagal membaca/deserialize settings.json! Error: {ex.Message}");
                Console.WriteLine($"[DEBUG-SETTINGS] [READ ERROR] Gagal membaca/deserialize settings.json! Error: {ex.Message}");
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
        Debug.WriteLine($"[DEBUG-SETTINGS] ----------------------------------------");
        Debug.WriteLine($"[DEBUG-SETTINGS] [WRITE] Target File Path: '{_settingsFilePath}'");
        Debug.WriteLine($"[DEBUG-SETTINGS] [WRITE] Incoming Data -> IsDefaultViewA: {settings.IsDefaultViewA}, OllamaUrl: '{settings.OllamaUrl}', SelectedModel: '{settings.SelectedModel}'");
        Console.WriteLine($"[DEBUG-SETTINGS] ----------------------------------------");
        Console.WriteLine($"[DEBUG-SETTINGS] [WRITE] Target File Path: '{_settingsFilePath}'");
        Console.WriteLine($"[DEBUG-SETTINGS] [WRITE] Incoming Data -> IsDefaultViewA: {settings.IsDefaultViewA}, OllamaUrl: '{settings.OllamaUrl}', SelectedModel: '{settings.SelectedModel}'");

        try
        {
            string json = JsonSerializer.Serialize(settings, _jsonOptions);

            // Write Atomic via Temp File untuk mencegah corrupt jika aplikasi mendadak mati
            string tempFilePath = _settingsFilePath + ".tmp";
            File.WriteAllText(tempFilePath, json);
            File.Move(tempFilePath, _settingsFilePath, overwrite: true);

            Debug.WriteLine($"[DEBUG-SETTINGS] [WRITE SUCCESS] Content written to file:\n{json}");
            Console.WriteLine($"[DEBUG-SETTINGS] [WRITE SUCCESS] Content written to file:\n{json}");
            Debug.WriteLine($"[DEBUG-SETTINGS] ----------------------------------------");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DEBUG-SETTINGS] [WRITE CRITICAL FAIL] Gagal menulis ke file settings.json! StackTrace:\n{ex}");
            Console.WriteLine($"[DEBUG-SETTINGS] [WRITE CRITICAL FAIL] Gagal menulis ke file settings.json! StackTrace:\n{ex}");
            throw; // Re-throw agar caller dapat menangani error jika gagal
        }
    }
}