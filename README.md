# DNote

A lightweight, low-resource Windows desktop sticky note application with a skeuomorphic paper-style UI.

## Features

- **Multi-tab notes** — manage multiple sticky notes in one window
- **Always-on-top** — toggle pin to keep notes visible above all windows
- **Right-click management** — create, rename, delete tabs via context menu
- **6 color themes** — yellow, pink, blue, green, purple, orange
- **Double-click to edit** — click content area twice to enter edit mode
- **Auto-save** — content saves automatically (500ms debounce)
- **Window memory** — remembers position and size between sessions
- **Minimal footprint** — WPF native, ~30MB RAM usage

## Download

### Option 1: Installer (Recommended)

Download `DNote-Setup-1.0.0.exe` from [Releases](../../releases), run the installer.

### Option 2: Portable

Download `DNote.exe` from [Releases](../../releases) and run directly. No installation needed.

### Option 3: Framework-Dependent (Smallest)

Download the framework-dependent version (~286KB). Requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

## Build from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
git clone https://github.com/shanghuashuo/DNote.git
cd DNote
dotnet build DNote/DNote.csproj -c Release
```

### Publish

```bash
# Self-contained single file (~154MB, no runtime needed)
dotnet publish DNote/DNote.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/portable

# Framework-dependent (~286KB, needs .NET 8 Runtime)
dotnet publish DNote/DNote.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish/framework-dependent
```

### Create Installer (Optional)

Install [Inno Setup 6](https://jrsoftware.org/isinfo.php), then open `installer/DNote.iss` and compile.

## Usage

| Action | How |
|--------|-----|
| New note | Right-click tab bar → "新建便签" |
| Rename note | Right-click tab → "重命名" |
| Delete note | Right-click tab → "删除" |
| Change color | Right-click tab → "颜色" → select |
| Edit content | Double-click the content area |
| Toggle pin | Click the pin icon in title bar |
| Move window | Drag the title bar |
| Resize | Drag window edges/corners |

## Data Location

Notes are stored at `%APPDATA%\DNote\notes.json`. Back up this file to preserve your notes.

## Tech Stack

- C# / .NET 8 / WPF
- MVVM pattern (CommunityToolkit.Mvvm)
- JSON persistence (System.Text.Json)

## License

MIT
