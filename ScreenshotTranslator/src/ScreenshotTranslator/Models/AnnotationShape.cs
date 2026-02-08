using System.Windows;
using System.Windows.Media;

namespace ScreenshotTranslator.Models;

/// <summary>
/// 标注形状类型
/// </summary>
public enum ShapeType
{
    Rectangle,
    Ellipse,
    Arrow,
    Line,
    FreeDraw,
    Highlighter,
    Text,
    Mosaic
}

/// <summary>
/// 标注形状数据
/// </summary>
public class AnnotationShape
{
    public ShapeType Type { get; set; }
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public List<Point> Points { get; set; } = new();
    public Color StrokeColor { get; set; } = Colors.Red;
    public double StrokeThickness { get; set; } = 2;
    public string? Text { get; set; }
    public double FontSize { get; set; } = 14;
    public bool IsFilled { get; set; }
    public double Opacity { get; set; } = 1.0;
}
