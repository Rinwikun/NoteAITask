using System;

namespace NoteAITask.Models;

public class ShellNoteItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}