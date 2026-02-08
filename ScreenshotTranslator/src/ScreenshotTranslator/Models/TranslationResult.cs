namespace ScreenshotTranslator.Models;

/// <summary>
/// 翻译结果
/// </summary>
public class TranslationResult
{
    public bool Success { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public List<TranslationBlock> Blocks { get; set; } = new();
}

/// <summary>
/// 翻译文本块（对应图片中的文本区域）
/// </summary>
public class TranslationBlock
{
    public string OriginalText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
