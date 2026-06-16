using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DNote.Models;
using DNote.ViewModels;
using Microsoft.Win32;

namespace DNote;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private TabControl? _tabControl;
    private RichTextBox? _activeRichTextBox;
    private bool _isLoadingContent;
    private NoteItem? _currentNote;
    private static readonly string ImagesDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DNote", "images");

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
            AttachTabContextMenus();

            _activeRichTextBox = FindVisualChild<RichTextBox>(this);
            if (_activeRichTextBox is not null)
            {
                DataObject.AddPastingHandler(_activeRichTextBox, OnPaste);
                _activeRichTextBox.PreviewKeyDown += RichTextBox_PreviewKeyDown;
            }

            if (ViewModel.SelectedNote is not null)
                LoadContent();
        };

        Closing += (_, _) =>
        {
            SaveContent();
            ViewModel.FlushSave();
        };

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsTopmost))
                Topmost = ViewModel.IsTopmost;
            else if (e.PropertyName == nameof(MainViewModel.IsEditing))
                OnEditingChanged();
            else if (e.PropertyName == nameof(MainViewModel.SelectedNote))
                OnSelectedNoteChanged();
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
        ViewModel.IsEditing = false;
        if (e.ClickCount == 1)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private RichTextBox? GetRichTextBox()
    {
        var rtb = FindVisualChild<RichTextBox>(this);
        if (rtb is not null && rtb != _activeRichTextBox)
        {
            if (_activeRichTextBox is not null)
            {
                DataObject.RemovePastingHandler(_activeRichTextBox, OnPaste);
                _activeRichTextBox.PreviewKeyDown -= RichTextBox_PreviewKeyDown;
            }

            _activeRichTextBox = rtb;
            DataObject.AddPastingHandler(rtb, OnPaste);
            rtb.PreviewKeyDown += RichTextBox_PreviewKeyDown;
        }
        return rtb;
    }

    private void OnSelectedNoteChanged()
    {
        SaveContent();

        Dispatcher.BeginInvoke(new Action(() =>
        {
            var rtb = GetRichTextBox();
            if (rtb is not null)
                LoadContent();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnEditingChanged()
    {
        var rtb = GetRichTextBox();
        if (rtb is null) return;

        rtb.IsReadOnly = !ViewModel.IsEditing;

        if (ViewModel.IsEditing)
            rtb.Focus();
    }

    private void ContentArea_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!ViewModel.IsEditing)
        {
            ViewModel.IsEditing = true;
            e.Handled = true;
        }
    }

    private void RichTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsEditing)
            ViewModel.IsEditing = true;
    }

    private void RichTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SaveContent();
        ViewModel.IsEditing = false;
    }

    private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ViewModel.IsEditing && !_isLoadingContent)
            ViewModel.OnContentChanged();
    }

    private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ViewModel.IsEditing = false;
            e.Handled = true;
        }
    }

    private void Done_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsEditing = false;
    }

    private void LoadContent()
    {
        var rtb = GetRichTextBox();
        var note = ViewModel.SelectedNote;
        if (rtb is null || note is null) return;

        _isLoadingContent = true;
        _currentNote = note;

        ClearDocumentImages(rtb);
        rtb.Document.Blocks.Clear();

        if (!string.IsNullOrEmpty(note.RtfContent))
        {
            var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(note.RtfContent));
            range.Load(ms, DataFormats.Rtf);
        }
        else if (!string.IsNullOrEmpty(note.Content))
        {
            rtb.Document.Blocks.Add(new Paragraph(new Run(note.Content)));
        }

        if (note.Images.Count > 0)
        {
            var sorted = note.Images.OrderByDescending(img => img.ByteOffset).ToList();
            foreach (var noteImg in sorted)
            {
                if (!File.Exists(noteImg.Path)) continue;

                var offset = Math.Min(noteImg.ByteOffset, GetTextLength(rtb));
                var tp = GetTextPointerAtOffset(rtb.Document.ContentStart, offset);
                if (tp is null) continue;

                var paragraph = tp.Paragraph;
                if (paragraph is null) continue;

                var img = CreateImageElement(noteImg.Path);
                var container = new InlineUIContainer(img, tp);
            }
        }

        _isLoadingContent = false;
    }

    private void SaveContent()
    {
        var rtb = GetRichTextBox();
        var note = _currentNote;
        if (rtb is null || note is null) return;

        var images = new List<NoteImage>();
        foreach (var block in rtb.Document.Blocks)
        {
            if (block is not Paragraph para) continue;
            foreach (var inline in para.Inlines.ToList())
            {
                if (inline is InlineUIContainer ui && ui.Child is System.Windows.Controls.Image img
                    && img.Tag is string path)
                {
                    images.Add(new NoteImage
                    {
                        Path = path,
                        ByteOffset = GetOffsetAtPointer(inline.ContentStart)
                    });
                }
            }
        }
        note.Images = images;

        var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        using var ms = new MemoryStream();
        range.Save(ms, DataFormats.Rtf);
        note.RtfContent = System.Text.Encoding.UTF8.GetString(ms.ToArray());

        var text = range.Text?.Trim() ?? "";
        foreach (var _ in images)
        {
            var idx = text.IndexOf('\uFFFC');
            if (idx >= 0) text = text.Remove(idx, 1);
        }
        note.Content = text;
    }

    private System.Windows.Controls.Image CreateImageElement(string path)
    {
        var img = new System.Windows.Controls.Image
        {
            Stretch = System.Windows.Media.Stretch.Uniform,
            MaxWidth = 400,
            Cursor = Cursors.Hand,
            Tag = path
        };

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 400;
            bmp.EndInit();
            bmp.Freeze();
            img.Source = bmp;

            if (bmp.PixelWidth > 400)
            {
                img.Width = 400;
                img.Height = 400.0 * bmp.PixelHeight / bmp.PixelWidth;
            }
            else
            {
                img.Width = bmp.PixelWidth;
                img.Height = bmp.PixelHeight;
            }
        }
        catch { }

        img.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ClickCount >= 2)
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }
                catch { }
                e.Handled = true;
            }
        };

        return img;
    }

    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        var rtb = GetRichTextBox();
        if (rtb?.Selection is not TextSelection sel || sel.IsEmpty) return;
        var current = sel.GetPropertyValue(TextElement.FontWeightProperty);
        sel.ApplyPropertyValue(TextElement.FontWeightProperty,
            current is FontWeight fw && fw == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
        rtb.Focus();
    }

    private void Italic_Click(object sender, RoutedEventArgs e)
    {
        var rtb = GetRichTextBox();
        if (rtb?.Selection is not TextSelection sel || sel.IsEmpty) return;
        var current = sel.GetPropertyValue(TextElement.FontStyleProperty);
        sel.ApplyPropertyValue(TextElement.FontStyleProperty,
            current is FontStyle fs && fs == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
        rtb.Focus();
    }

    private void InsertImage_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "选择图片",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp|所有文件|*.*"
        };
        if (dlg.ShowDialog() == true)
            InsertImageFromFile(dlg.FileName);
        GetRichTextBox()?.Focus();
    }

    private void InsertImageFromFile(string filePath)
    {
        var rtb = GetRichTextBox();
        if (rtb is null) return;
        try
        {
            Directory.CreateDirectory(ImagesDir);
            var ext = Path.GetExtension(filePath);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var destPath = Path.Combine(ImagesDir, fileName);
            File.Copy(filePath, destPath, true);

            var img = CreateImageElement(destPath);
            var container = new InlineUIContainer(img, rtb.CaretPosition)
            {
                BaselineAlignment = BaselineAlignment.Bottom
            };

            var paragraph = rtb.CaretPosition.Paragraph;
            if (paragraph is null)
            {
                paragraph = new Paragraph();
                rtb.Document.Blocks.Add(paragraph);
            }

            if (rtb.Selection.IsEmpty)
            {
                paragraph.Inlines.Add(container);
                paragraph.Inlines.Add(new Run(" "));
            }
            else
            {
                rtb.Selection.Text = "";
                paragraph.Inlines.Add(container);
            }

            rtb.CaretPosition = container.ElementEnd;
        }
        catch { }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
        {
            var bmpSource = e.DataObject.GetData(DataFormats.Bitmap) as BitmapSource;
            if (bmpSource is null) { e.CancelCommand(); return; }

            try
            {
                Directory.CreateDirectory(ImagesDir);
                var fileName = Guid.NewGuid().ToString("N") + ".png";
                var filePath = Path.Combine(ImagesDir, fileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                    encoder.Save(fs);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    InsertImageFromFile(filePath);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch { }

            e.CancelCommand();
        }
    }

    private static int GetOffsetAtPointer(TextPointer pointer)
    {
        int offset = 0;
        var current = pointer.DocumentStart;
        while (current is not null && current.CompareTo(pointer) < 0)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                offset += current.GetTextRunLength(LogicalDirection.Forward);
            current = current.GetNextInsertionPosition(LogicalDirection.Forward);
        }
        return offset;
    }

    private static int GetTextLength(RichTextBox rtb)
    {
        var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        return range.Text?.Length ?? 0;
    }

    private static TextPointer? GetTextPointerAtOffset(TextPointer start, int offset)
    {
        var current = start;
        int accumulated = 0;
        while (current is not null)
        {
            var ctx = current.GetPointerContext(LogicalDirection.Forward);
            if (ctx == TextPointerContext.Text)
            {
                var runLen = current.GetTextRunLength(LogicalDirection.Forward);
                if (accumulated + runLen >= offset)
                    return current.GetPositionAtOffset(offset - accumulated);
                accumulated += runLen;
            }
            var next = current.GetNextInsertionPosition(LogicalDirection.Forward);
            if (next is null) break;
            if (ctx == TextPointerContext.ElementStart || ctx == TextPointerContext.ElementEnd)
            {
                if (accumulated >= offset) return current;
            }
            current = next;
        }
        return null;
    }

    private void AttachTabContextMenus()
    {
        _tabControl = FindVisualChild<TabControl>(this);
        if (_tabControl is null) return;

        _tabControl.ItemContainerGenerator.StatusChanged += (_, _) =>
        {
            if (_tabControl.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) return;
            for (int i = 0; i < _tabControl.Items.Count; i++)
            {
                if (_tabControl.ItemContainerGenerator.ContainerFromIndex(i) is TabItem tabItem)
                    tabItem.ContextMenu = CreateTabContextMenu();
            }
        };

        for (int i = 0; i < _tabControl.Items.Count; i++)
        {
            if (_tabControl.ItemContainerGenerator.ContainerFromIndex(i) is TabItem tabItem)
                tabItem.ContextMenu = CreateTabContextMenu();
        }
    }

    private ContextMenu CreateTabContextMenu()
    {
        var menu = new ContextMenu();

        var addItem = new MenuItem { Header = "新建便签" };
        addItem.Click += (_, _) => ViewModel.AddNoteCommand.Execute(null);
        menu.Items.Add(addItem);
        menu.Items.Add(new Separator());

        var renameItem = new MenuItem { Header = "重命名" };
        renameItem.Click += (_, _) => ShowRenameDialog();
        menu.Items.Add(renameItem);

        if (ViewModel.Notes.Count > 1)
        {
            var deleteItem = new MenuItem { Header = "删除" };
            deleteItem.Click += (_, _) => ViewModel.DeleteNoteCommand.Execute(null);
            menu.Items.Add(deleteItem);
        }
        menu.Items.Add(new Separator());

        var colorItem = new MenuItem { Header = "颜色" };
        var colors = new[] { ("淡黄", "#FFF9C4"), ("粉色", "#F8BBD0"), ("淡蓝", "#BBDEFB"), ("淡绿", "#C8E6C9"), ("淡紫", "#E1BEE7"), ("淡橙", "#FFE0B2") };
        foreach (var (name, hex) in colors)
        {
            var colorMenuItem = new MenuItem { Header = name, Tag = hex };
            colorMenuItem.Click += (_, _) => ViewModel.ChangeNoteColorCommand.Execute(hex);
            colorItem.Items.Add(colorMenuItem);
        }
        menu.Items.Add(colorItem);

        return menu;
    }

    private void ShowRenameDialog()
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

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindVisualChild<T>(child);
            if (result is not null) return result;
        }
        return null;
    }

    private static void ClearDocumentImages(RichTextBox rtb)
    {
        try
        {
            foreach (var block in rtb.Document.Blocks)
            {
                if (block is not Paragraph para) continue;
                foreach (var inline in para.Inlines)
                {
                    if (inline is InlineUIContainer ui && ui.Child is System.Windows.Controls.Image img)
                    {
                        img.Source = null;
                    }
                }
            }
        }
        catch { }
    }
}
