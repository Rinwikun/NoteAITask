using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Services;

namespace NoteAITask.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };
    private readonly AppSettingsService _settingsService = new();
    private Timer? _statusCheckTimer;

    [ObservableProperty]
    private object? _currentPage;
    [ObservableProperty]
    private string _systemOllamaStatusText = "Ollama: Checking...";
    [ObservableProperty]
    private string _systemOllamaStatusColor = "#FAB387";
    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // WARNA BACKGROUND TOMBOL DINAMIS
    public string NoteTabBg => SelectedTabIndex == 0 ? "#89B4FA" : "#313244";
    public string NoteTabFg => SelectedTabIndex == 0 ? "#11111B" : "White";

    public string ShellTabBg => SelectedTabIndex == 1 ? "#89B4FA" : "#313244";
    public string ShellTabFg => SelectedTabIndex == 1 ? "#11111B" : "White";

    public string AITabBg => SelectedTabIndex == 2 ? "#89B4FA" : "#313244";
    public string AITabFg => SelectedTabIndex == 2 ? "#11111B" : "White";

    public string SettingsTabBg => SelectedTabIndex == 3 ? "#89B4FA" : "#313244";
    public string SettingsTabFg => SelectedTabIndex == 3 ? "#11111B" : "White";

    public string AboutTabBg => SelectedTabIndex == 4 ? "#FAB387" : "#313244";
    public string AboutTabFg => SelectedTabIndex == 4 ? "#11111B" : "#FAB387";

    // View Models
    private readonly NoteViewModel _noteViewModel = new();
    private readonly NoteShellViewModel _shellViewModel = new();
    private readonly NoteAIViewModel _aiViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();
    private readonly AboutViewModel _aboutViewModel = new();

    // Views
    private readonly Views.NoteView _noteView;
    private readonly Views.NoteShellView _shellView;
    private readonly Views.NoteAIView _aiView;
    private readonly Views.SettingsView _settingsView;
    private readonly Views.AboutView _aboutView;

    public MainWindowViewModel()
    {
        _noteView = new Views.NoteView { DataContext = _noteViewModel };
        _shellView = new Views.NoteShellView { DataContext = _shellViewModel };
        _aiView = new Views.NoteAIView { DataContext = _aiViewModel };
        _settingsView = new Views.SettingsView { DataContext = _settingsViewModel };
        _noteViewModel.IsViewA = _settingsViewModel.IsDefaultViewA;
        _aboutView = new Views.AboutView { DataContext = _aboutViewModel };
        CurrentPage = _noteView;
        StartOllamaStatusMonitoring();
    }
    private void UpdateTabProperties()
    {
        OnPropertyChanged(nameof(NoteTabBg));
        OnPropertyChanged(nameof(NoteTabFg));
        OnPropertyChanged(nameof(ShellTabBg));
        OnPropertyChanged(nameof(ShellTabFg));
        OnPropertyChanged(nameof(AITabBg));
        OnPropertyChanged(nameof(AITabFg));
        OnPropertyChanged(nameof(SettingsTabBg));
        OnPropertyChanged(nameof(SettingsTabFg));
        OnPropertyChanged(nameof(AboutTabBg));
        OnPropertyChanged(nameof(AboutTabFg));
    }
    private void StartOllamaStatusMonitoring()
    {
        // Pengecekan pertama langsung dijalankan, lalu diulang setiap 5 detik
        _statusCheckTimer = new Timer(async _ => await CheckOllamaHealthAsync(), null, 0, 5000);
    }
    private async Task CheckOllamaHealthAsync()
    {
        try
        {
            var settings = _settingsService.LoadSettings();
            string url = settings.OllamaUrl;

            var response = await _httpClient.GetAsync($"{url}/api/tags");
            if (response.IsSuccessStatusCode)
            {
                SystemOllamaStatusText = "🟢 Ollama: Connected";
                SystemOllamaStatusColor = "#A6E3A1"; // Hijau
            }
            else
            {
                SystemOllamaStatusText = "🔴 Ollama: Server Error";
                SystemOllamaStatusColor = "#F38BA8"; // Merah
            }
        }
        catch
        {
            SystemOllamaStatusText = "🔴 Ollama: Offline / Disconnected";
            SystemOllamaStatusColor = "#F38BA8"; // Merah
        }
    }

    [RelayCommand]
    private void NavigateToNote()
    {
        SelectedTabIndex = 0;
        CurrentPage = _noteView;
        UpdateTabProperties();
    }

    [RelayCommand]
    private void NavigateToShell()
    {
        SelectedTabIndex = 1;
        CurrentPage = _shellView;
        UpdateTabProperties();
    }

    [RelayCommand]
    private void NavigateToAI()
    {
        SelectedTabIndex = 2;
        CurrentPage = _aiView;
        UpdateTabProperties();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        SelectedTabIndex = 3;
        CurrentPage = _settingsView;
        UpdateTabProperties();
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
        SelectedTabIndex = 4;
        CurrentPage = _aboutView;
        UpdateTabProperties();
    }
}