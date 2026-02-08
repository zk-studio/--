namespace ScreenshotTranslator.Models;

/// <summary>
/// 截图历史记录项
/// </summary>
public class ScreenshotItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FilePath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public DateTime CaptureTime { get; set; } = DateTime.Now;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? OcrText { get; set; }
    public string? TranslatedText { get; set; }
    public bool IsPinned { get; set; }
}
