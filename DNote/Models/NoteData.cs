using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DNote.Models;

public class NoteData
{
    [JsonPropertyName("notes")]
    public List<NoteItem> Notes { get; set; } = new();

    [JsonPropertyName("windowBounds")]
    public WindowBounds? WindowBounds { get; set; }

    [JsonPropertyName("isTopmost")]
    public bool IsTopmost { get; set; } = true;
}

public class WindowBounds
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; } = 300;

    [JsonPropertyName("height")]
    public double Height { get; set; } = 400;
}
