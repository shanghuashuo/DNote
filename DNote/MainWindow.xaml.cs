using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DNote.ViewModels;

namespace DNote;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private TabControl? _tabControl;

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

        var deleteItem = new MenuItem { Header = "删除" };
        deleteItem.Click += (_, _) => ViewModel.DeleteNoteCommand.Execute(null);
        menu.Items.Add(deleteItem);

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
}
