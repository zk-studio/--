using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenshotTranslator.Helpers;

/// <summary>
/// 图片处理辅助类
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// 将 BitmapSource 保存为文件
    /// </summary>
    public static void SaveBitmapSource(BitmapSource bitmap, string filePath, string format = "png", int jpegQuality = 90)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        BitmapEncoder encoder = format.ToLower() switch
        {
            "jpg" or "jpeg" => new JpegBitmapEncoder { QualityLevel = jpegQuality },
            "bmp" => new BmpBitmapEncoder(),
            _ => new PngBitmapEncoder()
        };

        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        encoder.Save(fs);
    }

    /// <summary>
    /// 从文件加载 BitmapImage
    /// </summary>
    public static BitmapImage LoadBitmapImage(string filePath)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// 裁剪 BitmapSource
    /// </summary>
    public static BitmapSource CropBitmap(BitmapSource source, Int32Rect rect)
    {
        if (rect.X < 0) rect.X = 0;
        if (rect.Y < 0) rect.Y = 0;
        if (rect.X + rect.Width > source.PixelWidth)
            rect.Width = source.PixelWidth - rect.X;
        if (rect.Y + rect.Height > source.PixelHeight)
            rect.Height = source.PixelHeight - rect.Y;

        if (rect.Width <= 0 || rect.Height <= 0)
            return source;

        var cropped = new CroppedBitmap(source, rect);
        cropped.Freeze();
        return cropped;
    }

    /// <summary>
    /// 创建缩略图
    /// </summary>
    public static BitmapSource CreateThumbnail(BitmapSource source, int maxWidth = 200, int maxHeight = 150)
    {
        double scaleX = (double)maxWidth / source.PixelWidth;
        double scaleY = (double)maxHeight / source.PixelHeight;
        double scale = Math.Min(scaleX, scaleY);

        if (scale >= 1.0) return source;

        var transform = new ScaleTransform(scale, scale);
        var thumbnail = new TransformedBitmap(source, transform);
        thumbnail.Freeze();
        return thumbnail;
    }

    /// <summary>
    /// BitmapSource 转为字节数组
    /// </summary>
    public static byte[] BitmapSourceToBytes(BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// 对区域应用马赛克效果
    /// </summary>
    public static BitmapSource ApplyMosaic(BitmapSource source, Int32Rect rect, int blockSize = 10)
    {
        var cropped = CropBitmap(source, rect);
        int width = cropped.PixelWidth;
        int height = cropped.PixelHeight;
        int stride = width * 4;
        byte[] pixels = new byte[stride * height];
        cropped.CopyPixels(pixels, stride, 0);

        for (int y = 0; y < height; y += blockSize)
        {
            for (int x = 0; x < width; x += blockSize)
            {
                int avgB = 0, avgG = 0, avgR = 0, avgA = 0, count = 0;
                for (int dy = 0; dy < blockSize && y + dy < height; dy++)
                {
                    for (int dx = 0; dx < blockSize && x + dx < width; dx++)
                    {
                        int idx = (y + dy) * stride + (x + dx) * 4;
                        avgB += pixels[idx];
                        avgG += pixels[idx + 1];
                        avgR += pixels[idx + 2];
                        avgA += pixels[idx + 3];
                        count++;
                    }
                }
                avgB /= count; avgG /= count; avgR /= count; avgA /= count;

                for (int dy = 0; dy < blockSize && y + dy < height; dy++)
                {
                    for (int dx = 0; dx < blockSize && x + dx < width; dx++)
                    {
                        int idx = (y + dy) * stride + (x + dx) * 4;
                        pixels[idx] = (byte)avgB;
                        pixels[idx + 1] = (byte)avgG;
                        pixels[idx + 2] = (byte)avgR;
                        pixels[idx + 3] = (byte)avgA;
                    }
                }
            }
        }

        var result = BitmapSource.Create(width, height, cropped.DpiX, cropped.DpiY,
            PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }
}
