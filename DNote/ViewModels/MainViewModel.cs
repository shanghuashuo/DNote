using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DNote.Models;
using DNote.Services;

namespace DNote.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly JsonNoteStore _store = new();
    private readonly DispatcherTimer _saveTimer;
    private NoteData _data;

    [ObservableProperty]
    private NoteItem? _selectedNote;

    [ObservableProperty]
    private bool _isTopmost = true;

    [ObservableProperty]
    private bool _isEditing;

    public ObservableCollection<NoteItem> Notes { get; } = new();

    public MainViewModel()
    {
        _data = _store.Load();
        _isTopmost = _data.IsTopmost;

        foreach (var note in _data.Notes)
            Notes.Add(note);

        if (Notes.Count > 0)
            SelectedNote = Notes[0];

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveTimer.Tick += (_, _) => PerformSave();

        Notes.CollectionChanged += (_, _) => ScheduleSave();
    }

    partial void OnSelectedNoteChanged(NoteItem? value)
    {
        IsEditing = false;
    }

    [RelayCommand]
    private void AddNote()
    {
        var note = new NoteItem();
        Notes.Add(note);
        SelectedNote = note;
        ScheduleSave();
    }

    [RelayCommand]
    private void DeleteNote()
    {
        if (SelectedNote is null || Notes.Count <= 1) return;

        foreach (var img in SelectedNote.Images)
        {
            try { if (System.IO.File.Exists(img.Path)) System.IO.File.Delete(img.Path); }
            catch { }
        }

        var index = Notes.IndexOf(SelectedNote);
        Notes.Remove(SelectedNote);
        SelectedNote = Notes[Math.Min(index, Notes.Count - 1)];
        ScheduleSave();
    }

    [RelayCommand]
    private void RenameNote(string newTitle)
    {
        if (SelectedNote is null) return;
        SelectedNote.Title = newTitle;
        SelectedNote.UpdatedAt = DateTime.Now;
        ScheduleSave();
    }

    [RelayCommand]
    private void ChangeNoteColor(string color)
    {
        if (SelectedNote is null) return;
        SelectedNote.Color = color;
        SelectedNote.UpdatedAt = DateTime.Now;
        ScheduleSave();
    }

    partial void OnIsTopmostChanged(bool value)
    {
        ScheduleSave();
    }

    [RelayCommand]
    private void ToggleEditing()
    {
        IsEditing = !IsEditing;
    }

    public void OnContentChanged()
    {
        if (SelectedNote is not null)
        {
            SelectedNote.UpdatedAt = DateTime.Now;
            ScheduleSave();
        }
    }

    public void SaveWindowBounds(double x, double y, double width, double height)
    {
        _data.WindowBounds = new WindowBounds { X = x, Y = y, Width = width, Height = height };
        ScheduleSave();
    }

    public WindowBounds? GetWindowBounds() => _data.WindowBounds;

    public void FlushSave()
    {
        _saveTimer.Stop();
        if (Notes.Count > 0)
            PerformSave();
    }

    private void ScheduleSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void PerformSave()
    {
        _saveTimer.Stop();
        _data.Notes.Clear();
        foreach (var note in Notes)
            _data.Notes.Add(note);
        _data.IsTopmost = IsTopmost;
        _store.Save(_data);
    }
}
