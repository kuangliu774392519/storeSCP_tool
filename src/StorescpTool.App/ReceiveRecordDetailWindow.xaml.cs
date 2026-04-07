using System.Windows;
using StorescpTool.Core.Models;

namespace StorescpTool.App;

public partial class ReceiveRecordDetailWindow : Window
{
    public ReceiveRecordDetailWindow(ReceiveRecord record)
    {
        InitializeComponent();
        DataContext = record;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
