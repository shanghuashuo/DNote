# DNote Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a lightweight WPF sticky note app with multi-tab management, skeuomorphic paper UI, auto-save, and always-on-top toggle.

**Architecture:** MVVM pattern with single MainWindow, JsonNoteStore for persistence, and MainViewModel coordinating between Model and View. All code in C# targeting .NET 8.

**Tech Stack:** C# / .NET 8 / WPF / System.Text.Json / CommunityToolkit.Mvvm

---

## File Structure

```
DNote/
├── DNote.sln
├── DNote/
│   ├── DNote.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Models/
│   │   └── NoteItem.cs
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   ├── Services/
│   │   └── JsonNoteStore.cs
│   └── Resources/
│       └── Styles.xaml
```

---

### Task 1: Project Scaffolding

**Covers:** S7

**Files:**
- Create: `DNote.sln`
- Create: `DNote/DNote.csproj`
- Create: `DNote/App.xaml`
- Create: `DNote/App.xaml.cs`
- Create: `DNote/Models/` (directory)
- Create: `DNote/ViewModels/` (directory)
- Create: `DNote/Services/` (directory)
- Create: `DNote/Resources/` (directory)

- [ ] **Step 1: Create solution file**

```xml
<!-- DNote.sln -->
```

Actually, use `dotnet new sln` and `dotnet new wpf` when SDK is available. For now, create files manually.

- [ ] **Step 2: Create project file**

```xml
<!-- DNote/DNote.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <RootNamespace>DNote</RootNamespace>
    <AssemblyName>DNote</AssemblyName>
    <ApplicationIcon />
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Create App.xaml**

```xml
<!-- DNote/App.xaml -->
<Application x:Class="DNote.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources />
</Application>
```

- [ ] **Step 4: Create App.xaml.cs**

```csharp
// DNote/App.xaml.cs
using System.Windows;

namespace DNote;

public partial class App : Application
{
}
```

- [ ] **Step 5: Create directory structure**

Ensure these directories exist:
- `DNote/Models/`
- `DNote/ViewModels/`
- `DNote/Services/`
- `DNote/Resources/`

---

### Task 2: NoteItem Model

**Covers:** S4

**Files:**
- Create: `DNote/Models/NoteItem.cs`

- [ ] **Step 1: Create NoteItem model**

```csharp
// DNote/Models/NoteItem.cs
using System;
using System.Text.Json.Serialization;

namespace DNote.Models;

public class NoteItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = "新便签";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#FFF9C4";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
```

- [ ] **Step 2: Create NoteData wrapper model**

Add to the same file or create `DNote/Models/NoteData.cs`:

```csharp
// DNote/Models/NoteData.cs
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
```

---

### Task 3: JsonNoteStore Service

**Covers:** S3, S6

**Files:**
- Create: `DNote/Services/JsonNoteStore.cs`

- [ ] **Step 1: Create JsonNoteStore**

```csharp
// DNote/Services/JsonNoteStore.cs
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
            // Silently fail - auto-save should not crash the app
        }
    }
}
```

- [ ] **Step 2: Verify compilation**

When SDK is available, run: `dotnet build DNote/DNote.csproj`

---

### Task 4: MainViewModel

**Covers:** S5, S6

**Files:**
- Create: `DNote/ViewModels/MainViewModel.cs`

- [ ] **Step 1: Create MainViewModel**

```csharp
// DNote/ViewModels/MainViewModel.cs
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

    [RelayCommand]
    private void ToggleTopmost()
    {
        IsTopmost = !IsTopmost;
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
```

- [ ] **Step 2: Wire up Notes collection changes**

Add to constructor after loading notes:

```csharp
Notes.CollectionChanged += (_, _) => ScheduleSave();
```

---

### Task 5: Styles and Resources

**Covers:** S5

**Files:**
- Create: `DNote/Resources/Styles.xaml`

- [ ] **Step 1: Create Styles.xaml**

```xml
<!-- DNote/Resources/Styles.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Paper texture background -->
    <LinearGradientBrush x:Key="PaperBackground" StartPoint="0,0" EndPoint="1,1">
        <GradientStop Color="#FFFDE7" Offset="0"/>
        <GradientStop Color="#FFF9C4" Offset="1"/>
    </LinearGradientBrush>

    <!-- Note color presets -->
    <SolidColorBrush x:Key="NoteColorYellow" Color="#FFF9C4"/>
    <SolidColorBrush x:Key="NoteColorPink" Color="#F8BBD0"/>
    <SolidColorBrush x:Key="NoteColorBlue" Color="#BBDEFB"/>
    <SolidColorBrush x:Key="NoteColorGreen" Color="#C8E6C9"/>
    <SolidColorBrush x:Key="NoteColorPurple" Color="#E1BEE7"/>
    <SolidColorBrush x:Key="NoteColorOrange" Color="#FFE0B2"/>

    <!-- Title bar button style -->
    <Style x:Key="TitleBarButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="30"/>
        <Setter Property="Height" Value="28"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Foreground" Value="#5D4037"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}" 
                            CornerRadius="3"
                            Padding="6,4">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#20000000"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter Property="Background" Value="#30000000"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Pin button style (toggle) -->
    <Style x:Key="PinButtonStyle" TargetType="ToggleButton">
        <Setter Property="Width" Value="30"/>
        <Setter Property="Height" Value="28"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Foreground" Value="#5D4037"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ToggleButton">
                    <Border x:Name="border" Background="{TemplateBinding Background}" 
                            CornerRadius="3" Padding="6,4">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#20000000"/>
                        </Trigger>
                        <Trigger Property="IsChecked" Value="True">
                            <Setter TargetName="border" Property="Background" Value="#30FF8A65"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Tab item style -->
    <Style TargetType="TabItem">
        <Setter Property="Padding" Value="12,6"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="#5D4037"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="TabItem">
                    <Border x:Name="border" 
                            Background="{TemplateBinding Background}"
                            BorderBrush="#30000000" 
                            BorderThickness="0,0,0,2"
                            Padding="{TemplateBinding Padding}"
                            CornerRadius="4,4,0,0">
                        <ContentPresenter x:Name="content" 
                                          ContentSource="Header"
                                          HorizontalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#15000000"/>
                        </Trigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="border" Property="Background" Value="#25000000"/>
                            <Setter TargetName="border" Property="BorderBrush" Value="#FF8A65"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Content TextBox style -->
    <Style x:Key="NoteContentStyle" TargetType="TextBox">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Foreground" Value="#3E2723"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontFamily" Value="Segoe UI"/>
        <Setter Property="Padding" Value="12"/>
        <Setter Property="AcceptsReturn" Value="True"/>
        <Setter Property="TextWrapping" Value="Wrap"/>
        <Setter Property="VerticalScrollBarVisibility" Value="Auto"/>
        <Setter Property="VerticalAlignment" Value="Stretch"/>
    </Style>

    <!-- Read-only TextBlock style -->
    <Style x:Key="NoteTextStyle" TargetType="TextBlock">
        <Setter Property="Foreground" Value="#3E2723"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontFamily" Value="Segoe UI"/>
        <Setter Property="Padding" Value="12"/>
        <Setter Property="TextWrapping" Value="Wrap"/>
        <Setter Property="VerticalAlignment" Value="Stretch"/>
    </Style>

    <!-- Context menu style -->
    <Style TargetType="ContextMenu">
        <Setter Property="Background" value="#FFFAF0"/>
        <Setter Property="BorderBrush" Value="#D7CCC8"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="4"/>
    </Style>

    <Style TargetType="MenuItem">
        <Setter Property="Foreground" Value="#5D4037"/>
        <Setter Property="Padding" Value="8,4"/>
    </Style>
</ResourceDictionary>
```

---

### Task 6: MainWindow View

**Covers:** S5, S6

**Files:**
- Create: `DNote/MainWindow.xaml`
- Create: `DNote/MainWindow.xaml.cs`

- [ ] **Step 1: Create MainWindow.xaml**

```xml
<!-- DNote/MainWindow.xaml -->
<Window x:Class="DNote.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:DNote.ViewModels"
        Title="DNote"
        Width="300" Height="400"
        MinWidth="200" MinHeight="250"
        WindowStartupLocation="Manual"
        ResizeMode="CanResizeWithGrip"
        Background="Transparent"
        AllowsTransparency="True"
        WindowStyle="None">

    <Window.DataContext>
        <vm:MainViewModel/>
    </Window.DataContext>

    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <Border CornerRadius="8" 
            Background="{DynamicResource PaperBackground}"
            BorderBrush="#D7CCC8" 
            BorderThickness="1"
            Effect="{StaticResource WindowShadow}">
        <Border.Effect>
            <DropShadowEffect BlurRadius="15" ShadowDepth="2" Opacity="0.3" Color="#000000"/>
        </Border.Effect>

        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Title Bar -->
            <Grid Grid.Row="0" Background="#E8D5B7" MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <TextBlock Grid.Column="0" 
                           Text="DNote" 
                           Foreground="#5D4037" 
                           FontSize="13" FontWeight="SemiBold"
                           VerticalAlignment="Center" 
                           Margin="12,0"/>

                <StackPanel Grid.Column="1" Orientation="Horizontal">
                    <ToggleButton Style="{StaticResource PinButtonStyle}"
                                  IsChecked="{Binding IsTopmost}"
                                  Command="{Binding ToggleTopmostCommand}"
                                  ToolTip="置顶">
                        <TextBlock Text="&#x1F4CC;" FontSize="14"/>
                    </ToggleButton>
                    <Button Style="{StaticResource TitleBarButtonStyle}"
                            Click="Minimize_Click" ToolTip="最小化">
                        <TextBlock Text="&#x2013;" FontSize="14"/>
                    </Button>
                    <Button Style="{StaticResource TitleBarButtonStyle}"
                            Click="Close_Click" ToolTip="关闭">
                        <TextBlock Text="&#x2715;" FontSize="12"/>
                    </Button>
                </StackPanel>
            </Grid>

            <!-- Tab Bar -->
            <Grid Grid.Row="1">
                <TabControl ItemsSource="{Binding Notes}"
                            SelectedItem="{Binding SelectedNote}"
                            Background="Transparent"
                            BorderThickness="0"
                            Padding="0">
                    <TabControl.ItemTemplate>
                        <DataTemplate>
                            <TextBlock Text="{Binding Title}" 
                                       MaxWidth="80" 
                                       TextTrimming="CharacterEllipsis"/>
                        </DataTemplate>
                    </TabControl.ItemTemplate>

                    <!-- Tab right-click context menu -->
                    <TabControl.ItemContainerStyle>
                        <Style TargetType="TabItem" BasedOn="{StaticResource {x:Type TabItem}}">
                            <Setter Property="ContextMenu">
                                <Setter.Value>
                                    <ContextMenu>
                                        <MenuItem Header="新建便签" 
                                                  Command="{Binding DataContext.AddNoteCommand, RelativeSource={RelativeSource AncestorType=Window}}"/>
                                        <Separator/>
                                        <MenuItem Header="重命名" Click="Rename_Click"/>
                                        <MenuItem Header="删除" 
                                                  Command="{Binding DataContext.DeleteNoteCommand, RelativeSource={RelativeSource AncestorType=Window}}"/>
                                        <Separator/>
                                        <MenuItem Header="颜色">
                                            <MenuItem Header="淡黄" Click="Color_Click" Tag="#FFF9C4"/>
                                            <MenuItem Header="粉色" Click="Color_Click" Tag="#F8BBD0"/>
                                            <MenuItem Header="淡蓝" Click="Color_Click" Tag="#BBDEFB"/>
                                            <MenuItem Header="淡绿" Click="Color_Click" Tag="#C8E6C9"/>
                                            <MenuItem Header="淡紫" Click="Color_Click" Tag="#E1BEE7"/>
                                            <MenuItem Header="淡橙" Click="Color_Click" Tag="#FFE0B2"/>
                                        </MenuItem>
                                    </ContextMenu>
                                </Setter.Value>
                            </Setter>
                        </Style>
                    </TabControl.ItemContainerStyle>

                    <TabControl.ContentTemplate>
                        <DataTemplate>
                            <Grid Background="{Binding Color}">
                                <!-- Read-only view -->
                                <TextBlock Style="{StaticResource NoteTextStyle}"
                                           Text="{Binding Content, Mode=OneWay}"
                                           Cursor="IBeam"
                                           MouseLeftButtonDown="ContentText_MouseLeftDown"
                                           Visibility="{Binding DataContext.IsEditing, 
                                                        RelativeSource={RelativeSource AncestorType=Window},
                                                        Converter={StaticResource InverseBoolToVisibility}}"/>

                                <!-- Edit mode -->
                                <TextBox Style="{StaticResource NoteContentStyle}"
                                         Text="{Binding Content, UpdateSourceTrigger=PropertyChanged}"
                                         TextChanged="ContentText_TextChanged"
                                         LostFocus="ContentTextBox_LostFocus"
                                         Visibility="{Binding DataContext.IsEditing, 
                                                      RelativeSource={RelativeSource AncestorType=Window},
                                                      Converter={StaticResource BoolToVisibility}}"/>
                            </Grid>
                        </DataTemplate>
                    </TabControl.ContentTemplate>
                </TabControl>
            </Grid>
        </Grid>
    </Border>
</Window>
```

- [ ] **Step 2: Add BoolToVisibility converters to Styles.xaml**

Add to `DNote/Resources/Styles.xaml` before closing `</ResourceDictionary>`:

```xml
<!-- Converters -->
<BooleanToVisibilityConverter x:Key="BoolToVisibility"/>

<local:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibility"/>
```

- [ ] **Step 3: Create InverseBoolToVisibilityConverter**

```csharp
// DNote/Converters/InverseBoolToVisibilityConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DNote.Converters;

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Collapsed;
    }
}
```

- [ ] **Step 4: Create MainWindow.xaml.cs code-behind**

```csharp
// DNote/MainWindow.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DNote.ViewModels;

namespace DNote;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            var bounds = ViewModel.GetWindowBounds();
            if (bounds is not null)
            {
                Left = bounds.X;
                Top = bounds.Y;
                Width = bounds.Width;
                Height = bounds.Height;
            }
            Topmost = ViewModel.IsTopmost;
        };

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsTopmost))
                Topmost = ViewModel.IsTopmost;
        };

        LocationChanged += (_, _) => SaveBounds();
        SizeChanged += (_, _) => SaveBounds();
    }

    private void SaveBounds()
    {
        if (IsLoaded)
            ViewModel.SaveWindowBounds(Left, Top, Width, Height);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ContentText_MouseLeftDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            ViewModel.IsEditing = true;
            e.Handled = true;
        }
    }

    private void ContentTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.IsEditing = false;
    }

    private void ContentText_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.OnContentChanged();
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedNote is null) return;

        var dialog = new Window
        {
            Title = "重命名",
            Width = 250,
            Height = 120,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var stack = new StackPanel { Margin = new Thickness(10) };
        var textBox = new TextBox { Text = ViewModel.SelectedNote.Title, Margin = new Thickness(0, 0, 0, 10) };
        var okButton = new Button { Content = "确定", Width = 60, HorizontalAlignment = HorizontalAlignment.Right };

        okButton.Click += (_, _) =>
        {
            ViewModel.RenameNoteCommand.Execute(textBox.Text);
            dialog.Close();
        };

        textBox.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                ViewModel.RenameNoteCommand.Execute(textBox.Text);
                dialog.Close();
            }
        };

        stack.Children.Add(textBox);
        stack.Children.Add(okButton);
        dialog.Content = stack;
        dialog.Show();
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item && item.Tag is string color)
            ViewModel.ChangeNoteColorCommand.Execute(color);
    }
}
```

---

### Task 7: Update App.xaml with Converter Registration

**Covers:** S5

**Files:**
- Modify: `DNote/App.xaml`

- [ ] **Step 1: Update App.xaml with namespace and resource**

```xml
<Application x:Class="DNote.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:converters="clr-namespace:DNote.Converters"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
            <converters:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibility"/>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

### Task 8: Build and Verify

**Covers:** S3, S8

**Files:**
- None (verification only)

- [ ] **Step 1: Install .NET 8 SDK**

Download from: https://aka.ms/dotnet/8.0/sdk

- [ ] **Step 2: Restore packages and build**

```bash
cd D:\postgraduate\DNote
dotnet restore DNote/DNote.csproj
dotnet build DNote/DNote.csproj --configuration Release
```

Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Run the application**

```bash
dotnet run --project DNote/DNote.csproj
```

Expected: DNote window appears with paper-style UI, one default tab, can add/rename/delete tabs, double-click to edit, auto-saves to `%APPDATA%/DNote/notes.json`.

- [ ] **Step 4: Verify auto-save**

1. Add content to a note
2. Wait 1 second
3. Check `%APPDATA%/DNote/notes.json` exists and contains the content

- [ ] **Step 5: Verify window persistence**

1. Move and resize the window
2. Close and reopen DNote
3. Window should restore to previous position and size
