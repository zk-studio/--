using System.IO;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using ScreenshotTranslator.Helpers;
using BitmapDecoder = Windows.Graphics.Imaging.BitmapDecoder;

namespace ScreenshotTranslator.Services;

/// <summary>
/// OCR 文字识别服务（使用 Windows 内置 OCR，完全免费）
/// </summary>
public class OcrService
{
    /// <summary>
    /// 获取支持的 OCR 语言列表
    /// </summary>
    public static IReadOnlyList<string> GetAvailableLanguages()
    {
        return OcrEngine.AvailableRecognizerLanguages
            .Select(l => l.LanguageTag)
            .ToList();
    }

    /// <summary>
    /// 对图片进行 OCR 识别
    /// </summary>
    public async Task<OcrResult> RecognizeAsync(BitmapSource bitmapSource, string languageTag = "zh-Hans")
    {
        // 将 WPF BitmapSource 转为字节数组
        byte[] imageBytes = ImageHelper.BitmapSourceToBytes(bitmapSource);

        // 创建 Windows Runtime 的 SoftwareBitmap
        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(imageBytes);
            await writer.StoreAsync();
        }
        stream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(stream);
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

        // 创建 OCR 引擎
        OcrEngine? engine = null;
        var language = OcrEngine.AvailableRecognizerLanguages
            .FirstOrDefault(l => l.LanguageTag.Equals(languageTag, StringComparison.OrdinalIgnoreCase));

        if (language != null)
            engine = OcrEngine.TryCreateFromLanguage(language);
        
        engine ??= OcrEngine.TryCreateFromUserProfileLanguages();

        if (engine == null)
            throw new InvalidOperationException("无法创建 OCR 引擎，请确保系统安装了对应的语言包。");

        // 执行 OCR
        var result = await engine.RecognizeAsync(softwareBitmap);
        return new OcrResult
        {
            Text = result.Text,
            Lines = result.Lines.Select(l => new OcrLine
            {
                Text = l.Text,
                Words = l.Words.Select(w => new OcrWord
                {
                    Text = w.Text,
                    BoundingRect = new System.Windows.Rect(
                        w.BoundingRect.X, w.BoundingRect.Y,
                        w.BoundingRect.Width, w.BoundingRect.Height)
                }).ToList()
            }).ToList()
        };
    }
}

/// <summary>
/// OCR 识别结果
/// </summary>
public class OcrResult
{
    public string Text { get; set; } = string.Empty;
    public List<OcrLine> Lines { get; set; } = new();
}

public class OcrLine
{
    public string Text { get; set; } = string.Empty;
    public List<OcrWord> Words { get; set; } = new();
}

public class OcrWord
{
    public string Text { get; set; } = string.Empty;
    public System.Windows.Rect BoundingRect { get; set; }
}
