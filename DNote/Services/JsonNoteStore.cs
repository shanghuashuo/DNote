using System;
using System.IO;
using System.Text.Json;
using DNote.Models;

namespace DNote.Services;

public class JsonNoteStore
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DNote");

    private static readonly string FilePath = Path.Combine(AppDataDir, "notes.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NoteData Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new NoteData { Notes = { new NoteItem() } };

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<NoteData>(json, JsonOptions) ?? new NoteData();
        }
        catch
        {
            return new NoteData { Notes = { new NoteItem() } };
        }
    }

    public void Save(NoteData data)
    {
        try
        {
            Directory.CreateDirectory(AppDataDir);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
        }
    }
}
