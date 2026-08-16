using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteAITask.Models;
using NoteAITask.Services;

namespace NoteAITask.ViewModels;

public partial class NoteViewModel : ViewModelBase
{
    private readonly NoteStorageService _storageService = new();
    private readonly AppSettingsService _settingsService = new();

    // Mode View State
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewABackground))]
    [NotifyPropertyChangedFor(nameof(ViewBBackground))]
    private bool _isViewA;

    public string ViewABackground => IsViewA ? "#89B4FA" : "#313244";
    public string ViewBBackground => !IsViewA ? "#89B4FA" : "#313244";

    // State Tampilan B (0 = List Folder, 1 = List File, 2 = Editor Note Tampilan B)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(IsExplorerFolderLevel))]
    [NotifyPropertyChangedFor(nameof(IsExplorerFileLevel))]
    [NotifyPropertyChangedFor(nameof(IsExplorerEditorLevel))]
    [NotifyPropertyChangedFor(nameof(CurrentExplorerPath))]
    private int _explorerLevel = 0;

    public bool CanGoBack => ExplorerLevel > 0;
    public bool IsExplorerFolderLevel => ExplorerLevel == 0;
    public bool IsExplorerFileLevel => ExplorerLevel == 1;
    public bool IsExplorerEditorLevel => ExplorerLevel == 2;

    // Status Loading Simpan & Rename Dialog
    [ObservableProperty]
    private bool _isSaving = false;

    [ObservableProperty]
    private bool _isRenameOpen = false;

    [ObservableProperty]
    private string _renameInputText = string.Empty;

    private bool _isRenamingFolder = false;

    // STATUS DRAFT & WARNING POPUP
    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    [ObservableProperty]
    private bool _isUnsavedWarningOpen = false;

    private NoteItem? _pendingNoteToSelect = null;

    // Collections
    [ObservableProperty]
    private ObservableCollection<NoteFolder> _folders = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentExplorerPath))]
    private NoteFolder? _selectedFolder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentExplorerPath))]
    private NoteItem? _selectedNote;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private string _editorContent = string.Empty;

    // Undo / Redo Stacks & Tracking Variables
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private bool _isInternalTextChange = false;
    private string _lastSnapshotText = string.Empty;
    private string _savedTitleSnapshot = string.Empty;
    private string _savedContentSnapshot = string.Empty;
    private string _currentNoteFilePath = string.Empty;

    public NoteViewModel()
    {
        var settings = _settingsService.LoadSettings();
        _isViewA = settings.IsDefaultViewA;
        LoadData();
    }

    public void LoadData(string? targetFolderName = null, string? targetNoteTitle = null)
    {
        targetFolderName ??= SelectedFolder?.Name;
        targetNoteTitle ??= SelectedNote?.Title;

        var data = _storageService.GetAllFoldersWithNotes();

        _isInternalTextChange = true;
        Folders.Clear();
        foreach (var folder in data)
        {
            Folders.Add(folder);
        }

        NoteFolder? folderToSelect = Folders.FirstOrDefault(f => f.Name == targetFolderName)
                                    ?? Folders.FirstOrDefault();

        if (folderToSelect != null)
        {
            SelectedFolder = folderToSelect;

            NoteItem? noteToSelect = folderToSelect.Notes.FirstOrDefault(n => n.Title == targetNoteTitle)
                                    ?? folderToSelect.Notes.FirstOrDefault();

            if (noteToSelect != null)
            {
                SelectedNote = noteToSelect;
            }
        }
        _isInternalTextChange = false;
    }

    partial void OnSelectedFolderChanged(NoteFolder? value)
    {
        if (!_isInternalTextChange && value != null && value.Notes.Count > 0)
        {
            TrySelectNote(value.Notes[0]);
        }
    }

    partial void OnSelectedNoteChanged(NoteItem? value)
    {
        if (value != null)
        {
            if (!_isInternalTextChange && _currentNoteFilePath != value.FilePath)
            {
                _undoStack.Clear();
                _redoStack.Clear();
            }

            _currentNoteFilePath = value.FilePath;

            bool wasInternal = _isInternalTextChange;
            _isInternalTextChange = true;

            EditorTitle = value.Title;
            EditorContent = value.Content;

            _savedTitleSnapshot = value.Title;
            _savedContentSnapshot = value.Content;
            _lastSnapshotText = value.Content;

            HasUnsavedChanges = false;
            _isInternalTextChange = wasInternal;
        }
    }

    partial void OnEditorTitleChanged(string value) => CheckDirtyState();

    partial void OnEditorContentChanging(string? oldValue, string newValue)
    {
        if (_isInternalTextChange || oldValue == null) return;

        CheckDirtyState();

        if (newValue.Length > 0 && oldValue.Length > 0)
        {
            char lastChar = newValue[^1];
            bool isWordBoundary = char.IsWhiteSpace(lastChar) ||
                                 char.IsPunctuation(lastChar) ||
                                 Math.Abs(newValue.Length - _lastSnapshotText.Length) >= 3;

            if (isWordBoundary && _lastSnapshotText != oldValue)
            {
                _undoStack.Push(_lastSnapshotText);
                _lastSnapshotText = oldValue;
                _redoStack.Clear();
            }
        }
    }

    private void CheckDirtyState()
    {
        if (!_isInternalTextChange)
        {
            HasUnsavedChanges = (EditorTitle != _savedTitleSnapshot) || (EditorContent != _savedContentSnapshot);
        }
    }

    public void TrySelectNote(NoteItem targetNote)
    {
        if (targetNote == SelectedNote) return;

        if (HasUnsavedChanges)
        {
            _pendingNoteToSelect = targetNote;
            IsUnsavedWarningOpen = true;
        }
        else
        {
            SelectedNote = targetNote;
        }
    }

    [RelayCommand]
    private void ConfirmDiscardChanges()
    {
        IsUnsavedWarningOpen = false;
        HasUnsavedChanges = false;

        if (_pendingNoteToSelect != null)
        {
            SelectedNote = _pendingNoteToSelect;
            _pendingNoteToSelect = null;
        }
    }

    [RelayCommand]
    private void CancelDiscardChanges()
    {
        IsUnsavedWarningOpen = false;
        _pendingNoteToSelect = null;
    }

    // --- COMMAND MODE VIEW SWITCHER ---
    [RelayCommand]
    private void SwitchToViewA() => IsViewA = true;

    [RelayCommand]
    private void SwitchToViewB() => IsViewA = false;

    // --- COMMAND EXPLORER TAMPILAN B ---
    [RelayCommand]
    private void OpenFolderExplorer(NoteFolder? folder)
    {
        if (folder != null)
        {
            SelectedFolder = folder;
            ExplorerLevel = 1; // Ke Level List File
        }
    }

    [RelayCommand]
    private void OpenFileExplorer(NoteItem? note)
    {
        if (note != null)
        {
            TrySelectNote(note);
            ExplorerLevel = 2; // Buka Editor Khusus Tampilan B
        }
    }

    [RelayCommand]
    private void BackExplorer()
    {
        if (HasUnsavedChanges && ExplorerLevel == 2)
        {
            IsUnsavedWarningOpen = true;
            return;
        }

        if (ExplorerLevel > 0)
        {
            ExplorerLevel--;
        }
    }

    // --- COMMAND OPERASI FILE & FOLDER ---
    [RelayCommand]
    private async Task SaveCurrentNoteAsync()
    {
        if (SelectedFolder != null && !string.IsNullOrWhiteSpace(EditorTitle))
        {
            IsSaving = true;
            await Task.Delay(300);

            string folderName = SelectedFolder.Name;
            string newTitle = EditorTitle.Trim();

            if (SelectedNote != null && SelectedNote.Title != newTitle)
            {
                _storageService.DeleteNote(SelectedNote.FilePath);
            }

            _storageService.SaveNote(folderName, newTitle, EditorContent);

            _savedTitleSnapshot = newTitle;
            _savedContentSnapshot = EditorContent;
            _lastSnapshotText = EditorContent;

            HasUnsavedChanges = false;

            LoadData(folderName, newTitle);

            IsSaving = false;
        }
    }

    [RelayCommand]
    private void CreateNewFolder()
    {
        string newFolderName = $"Folder Baru {Folders.Count + 1}";
        string defaultNoteTitle = "Note Baru 1";

        _storageService.SaveNote(newFolderName, defaultNoteTitle, "Tulis di sini...");
        LoadData(newFolderName, defaultNoteTitle);
    }

    [RelayCommand]
    private void CreateNewNote()
    {
        if (SelectedFolder != null)
        {
            string folderName = SelectedFolder.Name;
            string newNoteTitle = $"Note Baru {SelectedFolder.Notes.Count + 1}";

            _storageService.SaveNote(folderName, newNoteTitle, "");
            LoadData(folderName, newNoteTitle);
        }
    }

    [RelayCommand]
    private void DeleteFolder()
    {
        if (SelectedFolder != null)
        {
            _storageService.DeleteFolder(SelectedFolder.Name);
            LoadData();
        }
    }

    [RelayCommand]
    private void DeleteNote()
    {
        if (SelectedNote != null)
        {
            _storageService.DeleteNote(SelectedNote.FilePath);
            HasUnsavedChanges = false;
            LoadData();
        }
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undoStack.Count > 0)
        {
            _isInternalTextChange = true;
            _redoStack.Push(EditorContent);

            string previousState = _undoStack.Pop();
            EditorContent = previousState;
            _lastSnapshotText = previousState;

            _isInternalTextChange = false;
            CheckDirtyState();
        }
    }

    [RelayCommand]
    private void Redo()
    {
        if (_redoStack.Count > 0)
        {
            _isInternalTextChange = true;
            _undoStack.Push(EditorContent);

            string nextState = _redoStack.Pop();
            EditorContent = nextState;
            _lastSnapshotText = nextState;

            _isInternalTextChange = false;
            CheckDirtyState();
        }
    }

    [RelayCommand]
    private void ShowRenameFolderDialog()
    {
        if (SelectedFolder != null)
        {
            _isRenamingFolder = true;
            RenameInputText = SelectedFolder.Name;
            IsRenameOpen = true;
        }
    }

    [RelayCommand]
    private void ShowRenameNoteDialog()
    {
        if (SelectedNote != null)
        {
            _isRenamingFolder = false;
            RenameInputText = SelectedNote.Title;
            IsRenameOpen = true;
        }
    }

    [RelayCommand]
    private void ConfirmRename()
    {
        if (string.IsNullOrWhiteSpace(RenameInputText)) return;

        if (_isRenamingFolder && SelectedFolder != null)
        {
            string oldName = SelectedFolder.Name;
            string newName = RenameInputText.Trim();

            _storageService.RenameFolder(oldName, newName);
            LoadData(newName, SelectedNote?.Title);
        }
        else if (!_isRenamingFolder && SelectedFolder != null && SelectedNote != null)
        {
            string newTitle = RenameInputText.Trim();
            _storageService.DeleteNote(SelectedNote.FilePath);
            _storageService.SaveNote(SelectedFolder.Name, newTitle, EditorContent);
            LoadData(SelectedFolder.Name, newTitle);
        }

        IsRenameOpen = false;
    }

    [RelayCommand]
    private void CancelRename()
    {
        IsRenameOpen = false;
    }
    // Properti teks lokasi yang dinamis untuk Tampilan B
    public string CurrentExplorerPath
    {
        get
        {
            return ExplorerLevel switch
            {
                0 => "Lokasi: Root /",
                1 => $"Lokasi: Root / {SelectedFolder?.Name}",
                2 => $"Lokasi: Root / {SelectedFolder?.Name} / {SelectedNote?.Title}",
                _ => "Lokasi: Root /"
            };
        }
    }
}