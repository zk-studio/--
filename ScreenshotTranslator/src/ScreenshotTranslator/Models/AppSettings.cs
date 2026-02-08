namespace ScreenshotTranslator.Models;

/// <summary>
/// 应用程序设置
/// </summary>
public class AppSettings
{
    // 快捷键设置
    public string RegionCaptureHotkey { get; set; } = "Ctrl+Shift+A";
    public string FullScreenCaptureHotkey { get; set; } = "Ctrl+Shift+F";
    public string TranslateCaptureHotkey { get; set; } = "Ctrl+Shift+T";
    public string OcrCaptureHotkey { get; set; } = "Ctrl+Shift+O";

    // 保存设置
    public string SaveDirectory { get; set; } = string.Empty;
    public string SaveFormat { get; set; } = "png";  // png, jpg, bmp
    public int JpegQuality { get; set; } = 90;
    public bool AutoSave { get; set; } = false;
    public bool CopyToClipboard { get; set; } = true;

    // 翻译设置
    public string SourceLanguage { get; set; } = "auto";
    public string TargetLanguage { get; set; } = "zh";
    public string TranslationProvider { get; set; } = "MyMemory";

    // OCR 设置
    public string OcrLanguage { get; set; } = "zh-Hans";

    // 界面设置
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowNotification { get; set; } = true;
    public bool PlaySound { get; set; } = false;

    // 标注默认设置
    public string DefaultStrokeColor { get; set; } = "#FFFF0000";
    public double DefaultStrokeThickness { get; set; } = 2;
    public double DefaultFontSize { get; set; } = 14;

    // 历史记录
    public int MaxHistoryCount { get; set; } = 100;
    public bool KeepHistory { get; set; } = true;

    // 延时截图
    public int DelaySeconds { get; set; } = 0;

    // 自动更新
    public bool AutoCheckUpdate { get; set; } = true;
    public string GitHubRepo { get; set; } = "";
}
