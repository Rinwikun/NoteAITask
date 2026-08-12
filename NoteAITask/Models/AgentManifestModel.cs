using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NoteAITask.Models;

public class ManifestNode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "file";

    [JsonPropertyName("relativePath")]
    public string RelativePath { get; set; } = string.Empty;
}

public class AgentManifestPlan
{
    [JsonPropertyName("actionType")]
    public string ActionType { get; set; } = "WRITE"; // "READ" atau "WRITE"

    [JsonPropertyName("readCommand")]
    public string ReadCommand { get; set; } = string.Empty; // Perintah baca langsung jika actionType = READ

    [JsonPropertyName("targetRoot")]
    public string TargetRoot { get; set; } = string.Empty;

    [JsonPropertyName("rootName")]
    public string RootName { get; set; } = "project-root";

    [JsonPropertyName("nodes")]
    public List<ManifestNode> Nodes { get; set; } = new();
}
public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}