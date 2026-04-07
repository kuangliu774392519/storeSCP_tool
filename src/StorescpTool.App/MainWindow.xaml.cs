using System.IO;
using System.Windows;
using StorescpTool.Core.Models;
using StorescpTool.App.ViewModels;

namespace StorescpTool.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private GridLength _sidebarWidth = new(0.82, GridUnitType.Star);

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        await _viewModel.InitializeAsync();
    }

    private void SidebarToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        if (SidebarColumn.Width.Value > 0)
        {
            _sidebarWidth = SidebarColumn.Width;
        }

        SidebarPane.Visibility = Visibility.Collapsed;
        PaneSplitter.Visibility = Visibility.Collapsed;
        SidebarColumn.MinWidth = 0;
        SidebarColumn.Width = new GridLength(0);
        SplitterColumn.Width = new GridLength(0);
        SidebarToggleButton.Content = "显示左侧操作区";
    }

    private void SidebarToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        SidebarPane.Visibility = Visibility.Visible;
        PaneSplitter.Visibility = Visibility.Visible;
        SidebarColumn.MinWidth = 260;
        SidebarColumn.Width = _sidebarWidth.Value > 0 ? _sidebarWidth : new GridLength(0.82, GridUnitType.Star);
        SplitterColumn.Width = new GridLength(8);
        SidebarToggleButton.Content = "隐藏左侧操作区";
    }

    private void BrowseReceiveDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = BrowseForFolder(_viewModel.ReceiveDirectory, "选择接收目录");
        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            _viewModel.ReceiveDirectory = selectedPath;
        }
    }

    private void BrowseLogDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = BrowseForFolder(_viewModel.LogDirectory, "选择日志目录");
        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            _viewModel.LogDirectory = selectedPath;
        }
    }

    private void ReceiveRecordsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ReceiveRecordsDataGrid.SelectedItem is not ReceiveRecord record)
        {
            return;
        }

        var detailWindow = new ReceiveRecordDetailWindow(record)
        {
            Owner = this
        };

        detailWindow.ShowDialog();
    }

    private static string? BrowseForFolder(string currentPath, string title)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            var resolvedPath = Path.IsPathRooted(currentPath)
                ? currentPath
                : Path.Combine(AppContext.BaseDirectory, currentPath);

            if (Directory.Exists(resolvedPath))
            {
                dialog.SelectedPath = resolvedPath;
            }
        }

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            ? dialog.SelectedPath
            : null;
    }
}
