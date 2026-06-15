# DNote - Windows Sticky Note App Design Spec

## [S1] Problem
Need a lightweight, low-resource Windows desktop sticky note application with multi-tab management, always-on-top capability, skeuomorphic paper-style UI, and auto-save functionality.

## [S2] Solution overview
Single-window WPF application (C#/.NET) using MVVM pattern. Notes stored as JSON in `%APPDATA%/DNote/notes.json`. Paper-texture skeuomorphic design with multi-color Post-it style notes.

## [S3] Architecture
- **Pattern**: MVVM (Model-View-ViewModel)
- **Framework**: WPF (.NET 6+)
- **Storage**: JSON file with debounced auto-save (500ms)
- **Entry**: Single `MainWindow` with `TabControl` for note tabs

### Data flow
```
User edits → ViewModel property change → Debounced save → JsonNoteStore → notes.json
App startup → JsonNoteStore.Load() → ViewModel → View renders
```

## [S4] Data model
```json
{
  "notes": [
    {
      "id": "guid",
      "title": "string",
      "content": "string",
      "color": "#FFF9C4",
      "createdAt": "ISO8601",
      "updatedAt": "ISO8601"
    }
  ],
  "windowBounds": { "x": 0, "y": 0, "width": 300, "height": 400 },
  "isTopmost": true
}
```

## [S5] UI design
- **Custom title bar**: App name "DNote" left-aligned; pin toggle, minimize, close buttons right-aligned
- **Tab bar**: TabControl with right-click context menu (New / Rename / Delete)
- **Edit area**: Double-click to enter edit mode; TextBox with paper-texture background
- **Color selection**: 6 preset colors in tab right-click menu (Yellow #FFF9C4, Pink #F8BBD0, Blue #BBDEFB, Green #C8E6C9, Purple #E1BEE7, Orange #FFE0B2)
- **Window**: Resizable, minimum size 200×250, remembers position and size

## [S6] Interaction logic
- **Auto-save**: Debounced 500ms on content change → write JSON
- **Pin toggle**: Click pin icon to toggle `Window.Topmost`
- **Tab operations**: Right-click context menu; new tab auto-focuses for editing
- **Startup restore**: Load window bounds and topmost state from JSON

## [S7] Project structure
```
DNote/
├── DNote.sln
├── DNote/
│   ├── DNote.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / MainWindow.xaml.cs
│   ├── Models/
│   │   └── NoteItem.cs
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   ├── Services/
│   │   └── JsonNoteStore.cs
│   └── Resources/
│       └── Styles.xaml
```

## [S8] Key constraints
- Single instance: only one DNote window at a time
- No external dependencies beyond .NET SDK
- Memory target: < 50MB RAM
- Startup time: < 1 second
