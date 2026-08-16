using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NoteAITask.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly AppSettingsService _settingsService = new();
    private CancellationTokenSource? _detectCts;
    // Default Note View Settings
    [ObservableProperty]
    private bool _isDefaultViewA;

    [ObservableProperty]
    private bool _isDefaultViewB;

    // Ollama Engine Settings
    [ObservableProperty]
    private string _ollamaUrl = "http://localhost:11434";

    [ObservableProperty]
    private string _selectedModel = "qwen2.5-coder:7b";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsManualModelEnabled))]
    [NotifyPropertyChangedFor(nameof(ModelLabelColor))]
    private bool _useAutoDetectModel = false;

    public bool IsManualModelEnabled => !UseAutoDetectModel;
    public string ModelLabelColor => UseAutoDetectModel ? "#585B70" : "White";

    [ObservableProperty]
    private ObservableCollection<string> _detectedModels = new();

    [ObservableProperty]
    private string? _selectedDetectedModel;

    [ObservableProperty]
    private string _detectedModelStatusText = string.Empty;

    [ObservableProperty]
    private string _connectionStatusText = "Status: Belum Dites";

    [ObservableProperty]
    private string _connectionStatusColor = "#A6ADC8";

    // System Prompt Template Settings
    [ObservableProperty]
    private string _systemPromptTemplate = string.Empty;

    // INDEPENDENT SAVE MESSAGES
    [ObservableProperty]
    private string _saveViewMessageText = string.Empty;

    [ObservableProperty]
    private string _saveEngineMessageText = string.Empty;

    [ObservableProperty]
    private string _savePromptMessageText = string.Empty;

    public SettingsViewModel()
    {
        var savedData = _settingsService.LoadSettings();

        _ollamaUrl = savedData.OllamaUrl;
        _selectedModel = savedData.SelectedModel;
        _useAutoDetectModel = savedData.UseAutoDetectModel;
        _systemPromptTemplate = savedData.SystemPromptTemplate;

        if (savedData.IsDefaultViewA)
        {
            _isDefaultViewA = true;
            _isDefaultViewB = false;
        }
        else
        {
            _isDefaultViewA = false;
            _isDefaultViewB = true;
        }

        if (UseAutoDetectModel)
        {
            _ = AutoDetectLocalModelAsync();
        }
    }

    // HANDLER SINKRONISASI RADIO BUTTON A & B + AUTO SAVE
    partial void OnIsDefaultViewAChanged(bool value)
    {
        if (value)
        {
            IsDefaultViewB = false;
        }
    }

    partial void OnIsDefaultViewBChanged(bool value)
    {
        if (value)
        {
            IsDefaultViewA = false;
        }
    }

    // 1. FUNGSI KHUSUS SIMPAN DEFAULT NOTE VIEW
    [RelayCommand]
    public void SaveViewSettings()
    {
        PersistViewSettings();          // <-- ganti dari PersistAllSettings()
        SaveViewMessageText = "✅ Tampilan Default Disimpan!";
        _ = ClearViewSaveMessageAsync();
    }

    // 2. FUNGSI KHUSUS SIMPAN OLLAMA ENGINE CONFIG
    [RelayCommand]
    public void SaveEngineSettings()
    {
        if (UseAutoDetectModel && !string.IsNullOrWhiteSpace(SelectedDetectedModel))
            SelectedModel = SelectedDetectedModel.Trim();

        if (string.IsNullOrWhiteSpace(SelectedModel))
            SelectedModel = "qwen2.5-coder:7b";

        try
        {
            PersistEngineSettings();    // <-- ganti dari PersistAllSettings()
            SaveEngineMessageText = $"✅ Engine & Model ({SelectedModel}) Disimpan!";
        }
        catch (Exception ex)
        {
            SaveEngineMessageText = $"🔴 Gagal menyimpan: {ex.Message}";
        }
        _ = ClearEngineSaveMessageAsync();
    }

    // 3. FUNGSI KHUSUS SIMPAN SYSTEM PROMPT TEMPLATE
    [RelayCommand]
    public void SavePromptSettings()
    {
        PersistPromptSettings();        // <-- ganti dari PersistAllSettings()
        SavePromptMessageText = "✅ System Prompt Berhasil Disimpan!";
        _ = ClearPromptSaveMessageAsync();
    }
    private void PersistViewSettings()
    {
        var current = _settingsService.LoadSettings(); 
        current.IsDefaultViewA = IsDefaultViewA;
        _settingsService.SaveSettings(BuildCurrentSnapshot());
    }

    private void PersistEngineSettings()
    {
        var current = _settingsService.LoadSettings();
        current.OllamaUrl = OllamaUrl.Trim();
        current.SelectedModel = SelectedModel.Trim();
        current.UseAutoDetectModel = UseAutoDetectModel; 
        _settingsService.SaveSettings(BuildCurrentSnapshot());
    }

    private void PersistPromptSettings()
    {
        var current = _settingsService.LoadSettings();
        current.SystemPromptTemplate = SystemPromptTemplate;
        _settingsService.SaveSettings(BuildCurrentSnapshot());
    }
    private AppSettingsData BuildCurrentSnapshot()
    {
        return new AppSettingsData
        {
            IsDefaultViewA = IsDefaultViewA,
            OllamaUrl = OllamaUrl.Trim(),
            SelectedModel = SelectedModel.Trim(),
            UseAutoDetectModel = UseAutoDetectModel,
            SystemPromptTemplate = SystemPromptTemplate
        };
    }
    private async Task ClearViewSaveMessageAsync()
    {
        await Task.Delay(2500);
        SaveViewMessageText = string.Empty;
    }

    private async Task ClearEngineSaveMessageAsync()
    {
        await Task.Delay(2500);
        SaveEngineMessageText = string.Empty;
    }

    private async Task ClearPromptSaveMessageAsync()
    {
        await Task.Delay(2500);
        SavePromptMessageText = string.Empty;
    }

    [RelayCommand]
    public void ResetDefaultPrompt()
    {
        var defaultData = new AppSettingsData();
        SystemPromptTemplate = defaultData.SystemPromptTemplate;
    }

    partial void OnUseAutoDetectModelChanged(bool value)
    {
        if (value)
        {
            _ = AutoDetectLocalModelAsync();
        }
        else
        {
            DetectedModelStatusText = string.Empty;
            DetectedModels.Clear();
        }
    }

    partial void OnSelectedDetectedModelChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value) && UseAutoDetectModel)
        {
            SelectedModel = value;
        }
    }

    private async Task AutoDetectLocalModelAsync()
    {
        _detectCts?.Cancel();
        _detectCts?.Dispose();
        var cts = new CancellationTokenSource();
        _detectCts = cts;

        DetectedModelStatusText = "🔍 Menemukan AI Lokal...";

        try
        {
            var response = await _httpClient.GetAsync($"{OllamaUrl}/api/tags", cts.Token);
            if (cts.IsCancellationRequested) return; // request ini sudah disusul request baru, buang hasilnya

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(json);
                var models = doc.RootElement.GetProperty("models");

                var freshModels = new List<string>();
                foreach (var model in models.EnumerateArray())
                {
                    string modelName = model.GetProperty("name").GetString() ?? "";
                    if (!string.IsNullOrEmpty(modelName))
                        freshModels.Add(modelName);
                }

                if (cts.IsCancellationRequested) return; // cek ulang setelah parsing, sebelum menyentuh UI state

                DetectedModels.Clear();
                foreach (var m in freshModels) DetectedModels.Add(m);

                if (DetectedModels.Count > 0)
                {
                    // PRESERVE pilihan user: match case-insensitive, jangan timpa kecuali benar2 tidak ada.
                    var match = DetectedModels.FirstOrDefault(
                        m => string.Equals(m, SelectedModel, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                        SelectedDetectedModel = match;
                    else
                    {
                        SelectedDetectedModel = DetectedModels[0];
                        SelectedModel = DetectedModels[0];
                    }

                    DetectedModelStatusText = $"🤖 Terdeteksi {DetectedModels.Count} AI Lokal di Ollama:";
                }
                else
                {
                    DetectedModelStatusText = "⚠️ Tidak ada model AI terinstal di Ollama lokal Anda.";
                }
            }
            else
            {
                DetectedModelStatusText = "🔴 Gagal terhubung ke Ollama.";
            }
        }
        catch (OperationCanceledException)
        {
            // Request lama dibatalkan oleh request baru — abaikan, bukan error.
        }
        catch
        {
            if (!cts.IsCancellationRequested)
                DetectedModelStatusText = "🔴 Ollama tidak aktif / Port URL salah.";
        }
    }

    [RelayCommand]
    public async Task CheckConnectionAsync()
    {
        ConnectionStatusText = "Memeriksa koneksi...";
        ConnectionStatusColor = "#FAB387";

        try
        {
            var response = await _httpClient.GetAsync($"{OllamaUrl}/api/tags");
            if (response.IsSuccessStatusCode)
            {
                ConnectionStatusText = "🟢 Terhubung ke Ollama!";
                ConnectionStatusColor = "#A6E3A1";

                if (UseAutoDetectModel)
                {
                    await AutoDetectLocalModelAsync();
                }
            }
            else
            {
                ConnectionStatusText = "🔴 Ollama merespon error!";
                ConnectionStatusColor = "#F38BA8";
            }
        }
        catch
        {
            ConnectionStatusText = "🔴 Tidak dapat terhubung!";
            ConnectionStatusColor = "#F38BA8";
        }
    }
}