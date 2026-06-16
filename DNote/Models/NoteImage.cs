using System.Text.Json.Serialization;

namespace DNote.Models;

public class NoteImage
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("offset")]
    public int ByteOffset { get; set; }
}
