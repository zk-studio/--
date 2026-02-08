using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ScreenshotTranslator.Helpers;
using ScreenshotTranslator.Models;

namespace ScreenshotTranslator.Views;

/// <summary>
/// 截图编辑窗口
/// </summary>
public partial class EditorWindow : Window
{
    private readonly BitmapSource _originalBitmap;
    private ShapeType _currentTool = ShapeType.Rectangle;
    private Color _currentColor = Colors.Red;
    private Point _drawStart;
    private bool _isDrawing;
    private UIElement? _currentShape;
    private readonly List<UIElement> _shapes = new();
    private readonly Stack<UIElement> _undoStack = new();

    public EditorWindow(BitmapSource bitmap)
    {
        InitializeComponent();
        _originalBitmap = bitmap;

        EditImage.Source = bitmap;
        DrawingCanvas.Width = bitmap.PixelWidth;
        DrawingCanvas.Height = bitmap.PixelHeight;

        KeyDown += EditorWindow_KeyDown;
    }

    private void EditorWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            Undo_Click(this, new RoutedEventArgs());
        else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
            Redo_Click(this, new RoutedEventArgs());
    }

    // ===== 工具选择 =====

    private void ShapeTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is string tag)
        {
            // 取消其他按钮
            foreach (var child in ((StackPanel)btn.Parent).Children)
            {
                if (child is ToggleButton other && other != btn)
                    other.IsChecked = false;
            }

            _currentTool = Enum.Parse<ShapeType>(tag);
        }
    }

    private void Color_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string colorStr)
        {
            _currentColor = (Color)ColorConverter.ConvertFromString(colorStr);
        }
    }

    // ===== 画布绘制 =====

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _drawStart = e.GetPosition(DrawingCanvas);
        _isDrawing = true;
        DrawingCanvas.CaptureMouse();

        if (_currentTool == ShapeType.FreeDraw || _currentTool == ShapeType.Highlighter)
        {
            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(_currentColor),
                StrokeThickness = _currentTool == ShapeType.Highlighter ? ThicknessSlider.Value * 4 : ThicknessSlider.Value,
                Opacity = _currentTool == ShapeType.Highlighter ? 0.4 : 1.0,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            polyline.Points.Add(_drawStart);
            DrawingCanvas.Children.Add(polyline);
            _currentShape = polyline;
        }
        else if (_currentTool == ShapeType.Text)
        {
            ShowTextInput(_drawStart);
            _isDrawing = false;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDrawing) return;
        var pos = e.GetPosition(DrawingCanvas);

        if (_currentTool == ShapeType.FreeDraw || _currentTool == ShapeType.Highlighter)
        {
            if (_currentShape is Polyline polyline)
            {
                polyline.Points.Add(pos);
            }
        }
        else
        {
            // 移除上一次的临时形状
            if (_currentShape != null)
                DrawingCanvas.Children.Remove(_currentShape);

            _currentShape = CreateShape(_drawStart, pos);
            if (_currentShape != null)
                DrawingCanvas.Children.Add(_currentShape);
        }
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDrawing) return;
        _isDrawing = false;
        DrawingCanvas.ReleaseMouseCapture();

        if (_currentShape != null)
        {
            _shapes.Add(_currentShape);
            _undoStack.Clear();
            _currentShape = null;
        }
    }

    private UIElement? CreateShape(Point start, Point end)
    {
        double x = Math.Min(start.X, end.X);
        double y = Math.Min(start.Y, end.Y);
        double w = Math.Abs(end.X - start.X);
        double h = Math.Abs(end.Y - start.Y);

        switch (_currentTool)
        {
            case ShapeType.Rectangle:
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = w, Height = h,
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = ThicknessSlider.Value
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                return rect;

            case ShapeType.Ellipse:
                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = w, Height = h,
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = ThicknessSlider.Value
                };
                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                return ellipse;

            case ShapeType.Line:
                return new Line
                {
                    X1 = start.X, Y1 = start.Y,
                    X2 = end.X, Y2 = end.Y,
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = ThicknessSlider.Value
                };

            case ShapeType.Arrow:
                return CreateArrow(start, end);

            default:
                return null;
        }
    }

    private UIElement CreateArrow(Point start, Point end)
    {
        var canvas = new Canvas();
        double thickness = ThicknessSlider.Value;
        var brush = new SolidColorBrush(_currentColor);

        // 箭头线
        canvas.Children.Add(new Line
        {
            X1 = start.X, Y1 = start.Y,
            X2 = end.X, Y2 = end.Y,
            Stroke = brush,
            StrokeThickness = thickness
        });

        // 箭头头部
        double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
        double headLen = 15;
        double headAngle = Math.PI / 6;

        var p1 = new Point(
            end.X - headLen * Math.Cos(angle - headAngle),
            end.Y - headLen * Math.Sin(angle - headAngle));
        var p2 = new Point(
            end.X - headLen * Math.Cos(angle + headAngle),
            end.Y - headLen * Math.Sin(angle + headAngle));

        var head = new Polygon
        {
            Fill = brush,
            Points = new PointCollection { end, p1, p2 }
        };
        canvas.Children.Add(head);

        return canvas;
    }

    private void ShowTextInput(Point position)
    {
        var textBox = new TextBox
        {
            FontSize = App.SettingsService.Settings.DefaultFontSize,
            Foreground = new SolidColorBrush(_currentColor),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(_currentColor),
            MinWidth = 100,
            Padding = new Thickness(4, 2, 4, 2),
            AcceptsReturn = true
        };

        Canvas.SetLeft(textBox, position.X);
        Canvas.SetTop(textBox, position.Y);
        DrawingCanvas.Children.Add(textBox);
        textBox.Focus();

        textBox.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                DrawingCanvas.Children.Remove(textBox);
                return;
            }

            var textBlock = new TextBlock
            {
                Text = textBox.Text,
                FontSize = textBox.FontSize,
                Foreground = textBox.Foreground,
                Padding = new Thickness(4, 2, 4, 2)
            };
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);

            DrawingCanvas.Children.Remove(textBox);
            DrawingCanvas.Children.Add(textBlock);
            _shapes.Add(textBlock);
        };
    }

    // ===== 撤销/重做 =====

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (_shapes.Count > 0)
        {
            var last = _shapes[^1];
            _shapes.RemoveAt(_shapes.Count - 1);
            DrawingCanvas.Children.Remove(last);
            _undoStack.Push(last);
        }
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        if (_undoStack.Count > 0)
        {
            var shape = _undoStack.Pop();
            DrawingCanvas.Children.Add(shape);
            _shapes.Add(shape);
        }
    }

    // ===== 导出操作 =====

    private BitmapSource RenderCanvas()
    {
        var rtb = new RenderTargetBitmap(
            (int)DrawingCanvas.Width, (int)DrawingCanvas.Height,
            96, 96, PixelFormats.Pbgra32);

        // 先渲染底图
        var drawingVisual = new DrawingVisual();
        using (var dc = drawingVisual.RenderOpen())
        {
            dc.DrawImage(_originalBitmap, new Rect(0, 0, DrawingCanvas.Width, DrawingCanvas.Height));
        }
        rtb.Render(drawingVisual);

        // 再渲染标注
        rtb.Render(DrawingCanvas);
        rtb.Freeze();
        return rtb;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        App.ClipboardService.CopyImage(RenderCanvas());
        MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
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
            ImageHelper.SaveBitmapSource(RenderCanvas(), dialog.FileName, ext);
        }
    }

    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = RenderCanvas();
        var pinWindow = new PinWindow(bitmap);
        pinWindow.Show();
    }

    private async void Ocr_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var result = await App.OcrService.RecognizeAsync(_originalBitmap, App.SettingsService.Settings.OcrLanguage);
            Mouse.OverrideCursor = null;

            if (!string.IsNullOrEmpty(result.Text))
            {
                var ocrWindow = new OcrResultWindow(result.Text, _originalBitmap);
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
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var ocrResult = await App.OcrService.RecognizeAsync(_originalBitmap, App.SettingsService.Settings.OcrLanguage);

            if (string.IsNullOrEmpty(ocrResult.Text))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("未识别到任何文字", "截图翻译", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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

            var translationWindow = new TranslationResultWindow(_originalBitmap, ocrResult, translationResult);
            translationWindow.Show();
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show($"翻译失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
