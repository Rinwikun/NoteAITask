using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Services;

namespace NoteAITask.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly AppSettingsService _settingsService = new();

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
        PersistAllSettings();
        SaveViewMessageText = "✅ Tampilan Default Disimpan!";
        _ = ClearViewSaveMessageAsync();
    }

    // 2. FUNGSI KHUSUS SIMPAN OLLAMA ENGINE CONFIG
[RelayCommand]
    public void SaveEngineSettings()
    {
        if (UseAutoDetectModel && !string.IsNullOrWhiteSpace(SelectedDetectedModel))
        {
            SelectedModel = SelectedDetectedModel.Trim();
        }

        if (string.IsNullOrWhiteSpace(SelectedModel))
        {
            SelectedModel = "qwen2.5-coder:7b";
        }

        PersistAllSettings();
        SaveEngineMessageText = $"✅ Engine & Model ({SelectedModel}) Disimpan!";
        _ = ClearEngineSaveMessageAsync();
    }

    // 3. FUNGSI KHUSUS SIMPAN SYSTEM PROMPT TEMPLATE
    [RelayCommand]
    public void SavePromptSettings()
    {
        PersistAllSettings();
        SavePromptMessageText = "✅ System Prompt Berhasil Disimpan!";
        _ = ClearPromptSaveMessageAsync();
    }

    // PENULISAN DOKUMEN SETTINGS LOKAL (SHARED DATA STATE)
    private void PersistAllSettings()
    {
        _settingsService.SaveSettings(new AppSettingsData
        {
            IsDefaultViewA = IsDefaultViewA,
            OllamaUrl = OllamaUrl.Trim(),
            SelectedModel = SelectedModel.Trim(),
            UseAutoDetectModel = UseAutoDetectModel,
            SystemPromptTemplate = SystemPromptTemplate
        });
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
        DetectedModelStatusText = "🔍 Menemukan AI Lokal...";
        DetectedModels.Clear();

        try
        {
            var response = await _httpClient.GetAsync($"{OllamaUrl}/api/tags");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var models = doc.RootElement.GetProperty("models");

                int count = models.GetArrayLength();
                if (count > 0)
                {
                    foreach (var model in models.EnumerateArray())
                    {
                        string modelName = model.GetProperty("name").GetString() ?? "";
                        if (!string.IsNullOrEmpty(modelName))
                        {
                            DetectedModels.Add(modelName);
                        }
                    }

                    if (DetectedModels.Contains(SelectedModel))
                    {
                        SelectedDetectedModel = SelectedModel;
                    }
                    else
                    {
                        SelectedDetectedModel = DetectedModels[0];
                        SelectedModel = DetectedModels[0];
                    }

                    DetectedModelStatusText = $"🤖 Terdeteksi {count} AI Lokal di Ollama:";
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
        catch
        {
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