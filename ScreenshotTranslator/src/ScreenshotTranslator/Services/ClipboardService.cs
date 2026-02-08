using System.Windows;
using System.Windows.Media.Imaging;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 剪贴板服务
/// </summary>
public class ClipboardService
{
    /// <summary>
    /// 复制图片到剪贴板
    /// </summary>
    public void CopyImage(BitmapSource bitmap)
    {
        try
        {
            Clipboard.SetImage(bitmap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"复制到剪贴板失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 复制文本到剪贴板
    /// </summary>
    public void CopyText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"复制到剪贴板失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取剪贴板中的图片
    /// </summary>
    public BitmapSource? GetImage()
    {
        try
        {
            if (Clipboard.ContainsImage())
                return Clipboard.GetImage();
        }
        catch { }
        return null;
    }
}
