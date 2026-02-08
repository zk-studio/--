using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenshotTranslator.Models;
using ScreenshotTranslator.Services;

namespace ScreenshotTranslator.Views;

public partial class TranslationResultWindow : Window
{
    private readonly TranslationResult _translationResult;

    public TranslationResultWindow(BitmapSource bitmap, OcrResult ocrResult, TranslationResult translationResult)
    {
        InitializeComponent();
        _translationResult = translationResult;

        OriginalImage.Source = bitmap;
        OriginalTextBox.Text = translationResult.OriginalText;
        TranslatedTextBox.Text = translationResult.TranslatedText;

        // 在图片上叠加翻译结果
        Loaded += (s, e) => RenderTranslationOverlay(bitmap);
    }

    private void RenderTranslationOverlay(BitmapSource bitmap)
    {
        if (_translationResult.Blocks.Count == 0) return;

        // 计算缩放比例
        double scaleX = OriginalImage.ActualWidth / bitmap.PixelWidth;
        double scaleY = OriginalImage.ActualHeight / bitmap.PixelHeight;
        double scale = Math.Min(scaleX, scaleY);

        foreach (var block in _translationResult.Blocks)
        {
            // 半透明背景 + 翻译文字
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(2),
                Child = new TextBlock
                {
                    Text = block.TranslatedText,
                    FontSize = 11,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = block.Width * scale
                }
            };

            Canvas.SetLeft(border, block.X * scale);
            Canvas.SetTop(border, block.Y * scale);
            TranslationOverlay.Children.Add(border);
        }
    }

    private void CopyOriginal_Click(object sender, RoutedEventArgs e)
    {
        App.ClipboardService.CopyText(_translationResult.OriginalText);
        MessageBox.Show("已复制原文", "提示");
    }

    private void CopyTranslation_Click(object sender, RoutedEventArgs e)
    {
        App.ClipboardService.CopyText(_translationResult.TranslatedText);
        MessageBox.Show("已复制翻译", "提示");
    }

    private void CopyAll_Click(object sender, RoutedEventArgs e)
    {
        string text = $"【原文】\n{_translationResult.OriginalText}\n\n【翻译】\n{_translationResult.TranslatedText}";
        App.ClipboardService.CopyText(text);
        MessageBox.Show("已复制全部", "提示");
    }
}
