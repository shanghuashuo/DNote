using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DNote.Models;

public class NoteItem : INotifyPropertyChanged
{
    private string _title = "新便签";
    private string _content = "";
    private string _rtfContent = "";
    private string _color = "#FFF9C4";

    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("content")]
    public string Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("rtfContent")]
    public string RtfContent
    {
        get => _rtfContent;
        set { _rtfContent = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("color")]
    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    [JsonPropertyName("images")]
    public List<NoteImage> Images { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
