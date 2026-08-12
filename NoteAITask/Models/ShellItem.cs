namespace NoteAITask.Models;

public class ShellItem
{
    public string Mode { get; set; } = string.Empty;
    public string LastWriteTime { get; set; } = string.Empty;
    public long? Length { get; set; }
    public string Name { get; set; } = string.Empty;

    // Properti pembantu untuk tampilan UI
    public string DisplayLength => Length.HasValue ? $"{Length.Value:N0} bytes" : "-";
}
