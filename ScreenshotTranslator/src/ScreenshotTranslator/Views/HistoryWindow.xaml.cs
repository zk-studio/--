using System.Windows;
using System.Windows.Controls;
using ScreenshotTranslator.Helpers;

namespace ScreenshotTranslator.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        InitializeComponent();
        RefreshList();
    }

    private void RefreshList()
    {
        HistoryList.ItemsSource = null;
        HistoryList.ItemsSource = App.HistoryService.Items;
        CountText.Text = $"共 {App.HistoryService.Items.Count} 条记录";
    }

    private void CopyItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            var item = App.HistoryService.Items.FirstOrDefault(i => i.Id == id);
            if (item != null && System.IO.File.Exists(item.FilePath))
            {
                var bitmap = ImageHelper.LoadBitmapImage(item.FilePath);
                App.ClipboardService.CopyImage(bitmap);
                MessageBox.Show("已复制到剪贴板", "提示");
            }
        }
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            App.HistoryService.Remove(id);
            RefreshList();
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要清空所有历史记录吗？", "确认",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            App.HistoryService.Clear();
            RefreshList();
        }
    }
}
