using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ScreenshotTranslator.Helpers;

namespace ScreenshotTranslator.Views;

public partial class PinWindow : Window
{
    private readonly BitmapSource _bitmap;
    private Point _dragStart;
    private bool _isDragging;

    public PinWindow(BitmapSource bitmap)
    {
        InitializeComponent();
        _bitmap = bitmap;
        PinnedImage.Source = bitmap;

        // 限制最大尺寸
        MaxWidth = SystemParameters.PrimaryScreenWidth * 0.8;
        MaxHeight = SystemParameters.PrimaryScreenHeight * 0.8;

        MouseLeftButtonDown += (s, e) =>
        {
            _dragStart = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
        };

        MouseMove += (s, e) =>
        {
            if (_isDragging)
            {
                var pos = e.GetPosition(this);
                Left += pos.X - _dragStart.X;
                Top += pos.Y - _dragStart.Y;
            }
        };

        MouseLeftButtonUp += (s, e) =>
        {
            _isDragging = false;
            ReleaseMouseCapture();
        };

        // 滚轮缩放
        MouseWheel += (s, e) =>
        {
            double scale = e.Delta > 0 ? 1.1 : 0.9;
            Width *= scale;
            Height *= scale;
        };
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        App.ClipboardService.CopyImage(_bitmap);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG|*.png|JPEG|*.jpg",
            DefaultExt = ".png",
            FileName = $"pin_{DateTime.Now:yyyyMMdd_HHmmss}"
        };
        if (dialog.ShowDialog() == true)
        {
            string ext = System.IO.Path.GetExtension(dialog.FileName).TrimStart('.').ToLower();
            ImageHelper.SaveBitmapSource(_bitmap, dialog.FileName, ext);
        }
    }

    private async void Ocr_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await App.OcrService.RecognizeAsync(_bitmap, App.SettingsService.Settings.OcrLanguage);
            if (!string.IsNullOrEmpty(result.Text))
            {
                var ocrWindow = new OcrResultWindow(result.Text, _bitmap);
                ocrWindow.Show();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OCR 失败: {ex.Message}");
        }
    }

    private async void Translate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var ocrResult = await App.OcrService.RecognizeAsync(_bitmap, App.SettingsService.Settings.OcrLanguage);
            if (string.IsNullOrEmpty(ocrResult.Text)) return;

            var segments = ocrResult.Lines.Select(l => (
                text: l.Text,
                x: l.Words.FirstOrDefault()?.BoundingRect.X ?? 0,
                y: l.Words.FirstOrDefault()?.BoundingRect.Y ?? 0,
                w: l.Words.Sum(w => w.BoundingRect.Width),
                h: l.Words.Max(w => w.BoundingRect.Height)
            )).ToList();

            var settings = App.SettingsService.Settings;
            var translationResult = await App.TranslationService.TranslateSegmentsAsync(
                segments, settings.SourceLanguage, settings.TargetLanguage);

            var translationWindow = new TranslationResultWindow(_bitmap, ocrResult, translationResult);
            translationWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"翻译失败: {ex.Message}");
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
