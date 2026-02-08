using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScreenshotTranslator.Helpers;

namespace ScreenshotTranslator.Views;

/// <summary>
/// 截图模式
/// </summary>
public enum CaptureMode
{
    Region,
    Translate,
    Ocr
}

/// <summary>
/// 截图区域选择窗口
/// </summary>
public partial class CaptureWindow : Window
{
    private readonly CaptureMode _mode;
    private BitmapSource? _screenBitmap;
    private Point _startPoint;
    private Point _endPoint;
    private bool _isSelecting;
    private bool _hasSelection;

    public CaptureWindow(CaptureMode mode)
    {
        InitializeComponent();
        _mode = mode;
        Loaded += CaptureWindow_Loaded;
        KeyDown += CaptureWindow_KeyDown;
        MouseDown += CaptureWindow_MouseDown;
        MouseMove += CaptureWindow_MouseMove;
        MouseUp += CaptureWindow_MouseUp;
    }

    private void CaptureWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 截取全屏
        _screenBitmap = App.CaptureService.CaptureFullScreen();
        BackgroundImage.Source = _screenBitmap;

        // 设置全屏遮罩
        MaskFull.Width = ActualWidth;
        MaskFull.Height = ActualHeight;
    }

    private void CaptureWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void CaptureWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _startPoint = e.GetPosition(this);
            _isSelecting = true;
            _hasSelection = false;
            MaskFull.Visibility = Visibility.Collapsed;
            SelectionBorder.Visibility = Visibility.Visible;
            ToolBar.Visibility = Visibility.Collapsed;
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            Close();
        }
    }

    private void CaptureWindow_MouseMove(object sender, MouseEventArgs e)
    {
        var currentPos = e.GetPosition(this);

        if (_isSelecting)
        {
            _endPoint = currentPos;
            UpdateSelectionRect();
        }

        UpdateMagnifier(currentPos);
    }

    private void CaptureWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isSelecting)
        {
            _isSelecting = false;
            _endPoint = e.GetPosition(this);

            var rect = GetSelectionRect();
            if (rect.Width > 5 && rect.Height > 5)
            {
                _hasSelection = true;
                MagnifierBorder.Visibility = Visibility.Collapsed;
                UpdateToolbarPosition();
                ToolBar.Visibility = Visibility.Visible;

                // 如果是自动模式（翻译/OCR），直接执行
                if (_mode == CaptureMode.Translate)
                {
                    Translate_Click(this, new RoutedEventArgs());
                }
                else if (_mode == CaptureMode.Ocr)
                {
                    Ocr_Click(this, new RoutedEventArgs());
                }
            }
        }
    }

    private Rect GetSelectionRect()
    {
        double x = Math.Min(_startPoint.X, _endPoint.X);
        double y = Math.Min(_startPoint.Y, _endPoint.Y);
        double w = Math.Abs(_endPoint.X - _startPoint.X);
        double h = Math.Abs(_endPoint.Y - _startPoint.Y);
        return new Rect(x, y, w, h);
    }

    private void UpdateSelectionRect()
    {
        var rect = GetSelectionRect();

        Canvas.SetLeft(SelectionBorder, rect.X);
        Canvas.SetTop(SelectionBorder, rect.Y);
        SelectionBorder.Width = rect.Width;
        SelectionBorder.Height = rect.Height;

        SizeText.Text = $"{(int)rect.Width} × {(int)rect.Height}";

        // 更新遮罩
        double w = ActualWidth;
        double h = ActualHeight;

        // 上方
        Canvas.SetLeft(MaskTop, 0);
        Canvas.SetTop(MaskTop, 0);
        MaskTop.Width = w;
        MaskTop.Height = rect.Y;

        // 下方
        Canvas.SetLeft(MaskBottom, 0);
        Canvas.SetTop(MaskBottom, rect.Y + rect.Height);
        MaskBottom.Width = w;
        MaskBottom.Height = h - rect.Y - rect.Height;

        // 左方
        Canvas.SetLeft(MaskLeft, 0);
        Canvas.SetTop(MaskLeft, rect.Y);
        MaskLeft.Width = rect.X;
        MaskLeft.Height = rect.Height;

        // 右方
        Canvas.SetLeft(MaskRight, rect.X + rect.Width);
        Canvas.SetTop(MaskRight, rect.Y);
        MaskRight.Width = w - rect.X - rect.Width;
        MaskRight.Height = rect.Height;
    }

    private void UpdateToolbarPosition()
    {
        var rect = GetSelectionRect();
        double toolbarY = rect.Y + rect.Height + 8;
        if (toolbarY + 40 > ActualHeight)
            toolbarY = rect.Y - 48;

        ToolBar.Margin = new Thickness(0, 0, 0, ActualHeight - toolbarY - 40);
    }

    private void UpdateMagnifier(Point pos)
    {
        if (_hasSelection) return;

        MagnifierBorder.Visibility = Visibility.Visible;

        // 放大镜位置
        double mx = pos.X + 20;
        double my = pos.Y + 20;
        if (mx + 140 > ActualWidth) mx = pos.X - 160;
        if (my + 140 > ActualHeight) my = pos.Y - 160;

        Canvas.SetLeft(MagnifierBorder, mx);
        Canvas.SetTop(MagnifierBorder, my);

        // 放大区域
        if (_screenBitmap != null)
        {
            int cx = (int)(pos.X * _screenBitmap.PixelWidth / ActualWidth);
            int cy = (int)(pos.Y * _screenBitmap.PixelHeight / ActualHeight);
            int cropSize = 35;
            int cropX = Math.Max(0, cx - cropSize / 2);
            int cropY = Math.Max(0, cy - cropSize / 2);

            var cropRect = new Int32Rect(cropX, cropY,
                Math.Min(cropSize, _screenBitmap.PixelWidth - cropX),
                Math.Min(cropSize, _screenBitmap.PixelHeight - cropY));

            if (cropRect.Width > 0 && cropRect.Height > 0)
            {
                var cropped = new CroppedBitmap(_screenBitmap, cropRect);
                MagnifierImage.Source = cropped;
            }
        }

        MagnifierInfo.Text = $"({(int)pos.X}, {(int)pos.Y})";
    }

    private BitmapSource? GetSelectedBitmap()
    {
        if (_screenBitmap == null) return null;

        var rect = GetSelectionRect();
        double scaleX = _screenBitmap.PixelWidth / ActualWidth;
        double scaleY = _screenBitmap.PixelHeight / ActualHeight;

        var cropRect = new Int32Rect(
            (int)(rect.X * scaleX),
            (int)(rect.Y * scaleY),
            (int)(rect.Width * scaleX),
            (int)(rect.Height * scaleY));

        return ImageHelper.CropBitmap(_screenBitmap, cropRect);
    }

    // ===== 工具栏按钮事件 =====

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap != null)
        {
            App.ClipboardService.CopyImage(bitmap);

            if (App.SettingsService.Settings.AutoSave)
            {
                var path = App.SettingsService.GenerateScreenshotPath();
                ImageHelper.SaveBitmapSource(bitmap, path);
            }
        }
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap != null)
        {
            App.ClipboardService.CopyImage(bitmap);
        }
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap != null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG 图片|*.png|JPEG 图片|*.jpg|BMP 图片|*.bmp",
                DefaultExt = ".png",
                FileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                string ext = System.IO.Path.GetExtension(dialog.FileName).TrimStart('.').ToLower();
                ImageHelper.SaveBitmapSource(bitmap, dialog.FileName, ext);
            }
        }
        Close();
    }

    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap != null)
        {
            var pinWindow = new PinWindow(bitmap);
            pinWindow.Show();
        }
        Close();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap != null)
        {
            var editor = new EditorWindow(bitmap);
            editor.Show();
        }
        Close();
    }

    private async void Ocr_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap == null) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var result = await App.OcrService.RecognizeAsync(bitmap, App.SettingsService.Settings.OcrLanguage);
            Mouse.OverrideCursor = null;

            if (!string.IsNullOrEmpty(result.Text))
            {
                var ocrWindow = new OcrResultWindow(result.Text, bitmap);
                Close();
                ocrWindow.Show();
            }
            else
            {
                MessageBox.Show("未识别到任何文字", "OCR 识别", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show($"OCR 识别失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Translate_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = GetSelectedBitmap();
        if (bitmap == null) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;

            // 先 OCR 识别
            var ocrResult = await App.OcrService.RecognizeAsync(bitmap, App.SettingsService.Settings.OcrLanguage);

            if (string.IsNullOrEmpty(ocrResult.Text))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("未识别到任何文字，无法翻译", "截图翻译", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 构建翻译段落
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

            Mouse.OverrideCursor = null;

            // 显示翻译结果窗口
            var translationWindow = new TranslationResultWindow(bitmap, ocrResult, translationResult);
            Close();
            translationWindow.Show();
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show($"翻译失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
