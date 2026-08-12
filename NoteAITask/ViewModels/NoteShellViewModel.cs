using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Models;
using NoteAITask.Services;

namespace NoteAITask.ViewModels;

public partial class NoteShellViewModel : ViewModelBase
{
    private readonly ShellService _shellService = new();

    [ObservableProperty]
    private ObservableCollection<ShellNoteItem> _shellNotes = new();

    [ObservableProperty]
    private string _inputName = string.Empty;

    [ObservableProperty]
    private string _inputCommand = string.Empty;

    [ObservableProperty]
    private string _terminalLog = "[System] Terminal Shell Service is ready...\n";

    [ObservableProperty]
    private bool _isExecuting = false;

    public NoteShellViewModel()
    {
        // Data Default/Contoh untuk Quick Run Commands
        ShellNotes.Add(new ShellNoteItem { Name = "Start Service Ollama", Command = "ollama serve" });
        ShellNotes.Add(new ShellNoteItem { Name = "Check Active Ports", Command = "netstat -ano | findstr 11434" });
        ShellNotes.Add(new ShellNoteItem { Name = "List Directory Full", Command = "Get-ChildItem" });
    }

    [RelayCommand]
    private void AddShellNote()
    {
        if (!string.IsNullOrWhiteSpace(InputName) && !string.IsNullOrWhiteSpace(InputCommand))
        {
            ShellNotes.Add(new ShellNoteItem
            {
                Name = InputName,
                Command = InputCommand
            });

            InputName = string.Empty;
            InputCommand = string.Empty;
            AppendLog($"[Info] Ditambahkan perintah baru: {InputName}");
        }
    }

    [RelayCommand]
    private async Task RunShellCommandAsync(ShellNoteItem? item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Command)) return;

        IsExecuting = true;
        AppendLog($"\n[Executing] > {item.Command}");

        var result = await _shellService.ExecuteCommandAsync(item.Command);

        if (result.IsSuccess)
        {
            AppendLog($"[SUCCESS]\n{result.Output}");
        }
        else
        {
            AppendLog($"[ERROR] Perintah Gagal!\nPenyebab: {result.ErrorMessage}");
        }

        IsExecuting = false;
    }

    [RelayCommand]
    private void DeleteShellNote(ShellNoteItem? item)
    {
        if (item != null)
        {
            ShellNotes.Remove(item);
            AppendLog($"[Info] Dihapus: {item.Name}");
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        TerminalLog = "[System] Log cleared.\n";
    }

    private void AppendLog(string message)
    {
        TerminalLog += $"{message}\n";
    }
}