using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NoteAITask.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "NoteAI Task Terminal";

    [ObservableProperty]
    private string _appVersion = "v3.2.2-beta (Cross-Platform Edition)";

    [ObservableProperty]
    private string _developerName = "Rinwikun";

    [ObservableProperty]
    private string _description = "Aplikasi manajemen catatan cerdas yang terintegrasi dengan Ollama Local AI Engine dan Dynamic Cross-Platform Terminal Execution Agent.";

    [RelayCommand]
    private void OpenGitHub()
    {
        string url = "https://github.com/Rinwikun";
        OpenBrowser(url);
    }

    [RelayCommand]
    private void OpenSupport()
    {
        // Ganti URL ini dengan link Saweria, Ko-fi, atau GitHub Sponsors Anda
        string url = "https://saweria.co/Rinwikun";
        OpenBrowser(url);
    }

    private void OpenBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch { }
    }
}