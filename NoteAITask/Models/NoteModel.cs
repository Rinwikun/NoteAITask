using System;
using System.Collections.Generic;
using System.Text;

namespace NoteAITask.Models;

// Model untuk satu file Note (.txt)
public class NoteItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
}

// Model untuk Folder yang berisi daftar Note
public class NoteFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public List<NoteItem> Notes { get; set; } = new();
}
