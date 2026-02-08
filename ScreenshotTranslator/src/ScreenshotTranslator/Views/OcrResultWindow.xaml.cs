using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScreenshotTranslator.Views;

public partial class OcrResultWindow : Window
{
    private readonly BitmapSource _bitmap;

    public OcrResultWindow(string text, BitmapSource bitmap)
    {
        InitializeComponent();
        _bitmap = bitmap;
        OcrTextBox.Text = text;
        PreviewImage.Source = bitmap;
    }

    private void CopyText_Click(object sender, RoutedEventArgs e)
    {
        App.ClipboardService.CopyText(OcrTextBox.Text);
        MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Translate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var settings = App.SettingsService.Settings;
            var result = await App.TranslationService.TranslateAsync(
                OcrTextBox.Text, settings.SourceLanguage, settings.TargetLanguage);
            Mouse.OverrideCursor = null;

            if (result.Success)
            {
                OcrTextBox.Text = $"【原文】\n{result.OriginalText}\n\n【翻译】\n{result.TranslatedText}";
            }
            else
            {
                MessageBox.Show($"翻译失败: {result.ErrorMessage}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show($"翻译失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
