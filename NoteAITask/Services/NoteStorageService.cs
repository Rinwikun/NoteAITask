using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoteAITask.Models;

namespace NoteAITask.Services;

public class NoteStorageService
{
    private readonly string _baseStoragePath;

    public NoteStorageService()
    {
        // Menyimpan catatan di folder AppData/NoteAITask/Notes
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _baseStoragePath = Path.Combine(appDataFolder, "NoteAITask", "Notes");

        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            CreateDefaultData(); // Buat folder & note contoh saat pertama kali jalan
        }
    }

    public List<NoteFolder> GetAllFoldersWithNotes()
    {
        var folders = new List<NoteFolder>();
        var dirInfo = new DirectoryInfo(_baseStoragePath);

        foreach (var subDir in dirInfo.GetDirectories())
        {
            var folder = new NoteFolder
            {
                Name = subDir.Name,
                FolderPath = subDir.FullName
            };

            foreach (var file in subDir.GetFiles("*.txt"))
            {
                folder.Notes.Add(new NoteItem
                {
                    Title = Path.GetFileNameWithoutExtension(file.Name),
                    Content = File.ReadAllText(file.FullName),
                    FilePath = file.FullName,
                    LastModified = file.LastWriteTime
                });
            }

            folders.Add(folder);
        }

        return folders;
    }

    public void SaveNote(string folderName, string title, string content)
    {
        string folderPath = Path.Combine(_baseStoragePath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, $"{title}.txt");
        File.WriteAllText(filePath, content);
    }
    public void RenameFolder(string oldFolderName, string newFolderName)
    {
        if (string.Equals(oldFolderName, newFolderName, StringComparison.OrdinalIgnoreCase))
            return;

        string oldPath = Path.Combine(_baseStoragePath, oldFolderName);
        string newPath = Path.Combine(_baseStoragePath, newFolderName);

        if (!Directory.Exists(oldPath))
            return;

        try
        {
            // Paksa garbage collection singkat agar file handle lokal dilepas
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (!Directory.Exists(newPath))
            {
                Directory.Move(oldPath, newPath);
            }
        }
        catch
        {
            // Fallback jika Directory.Move terkunci: Salin & Hapus Manual
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            foreach (string filePath in Directory.GetFiles(oldPath))
            {
                string fileName = Path.GetFileName(filePath);
                string destFile = Path.Combine(newPath, fileName);
                File.Copy(filePath, destFile, true);
            }

            Directory.Delete(oldPath, true);
        }
    }
    public void DeleteFolder(string folderName)
    {
        string folderPath = Path.Combine(_baseStoragePath, folderName);
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true); // Hapus folder beserta isinya
        }
    }
    public void DeleteNote(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private void CreateDefaultData()
    {
        // Folder 1: Content Youtube
        SaveNote("Content Youtube", "Persiapan Video", "1. Buka OBS Studio\n2. Record demo aplikasi Note AI Task");
        SaveNote("Content Youtube", "Judul dan Deskripsi", "Judul: Membuat App Desktop Note AI Task dengan Avalonia UI C#");

        // Folder 2: Coding & Script
        SaveNote("Coding & Script", "WakeOnLan Script", "powercfg /devicequery wake_online");
        SaveNote("Coding & Script", "CMD Tricks", "netstat -ano | findstr 11434");
    }
}
