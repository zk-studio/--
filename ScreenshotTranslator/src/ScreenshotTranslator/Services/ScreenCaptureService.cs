using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ScreenshotTranslator.Helpers;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 屏幕截图服务
/// </summary>
public class ScreenCaptureService
{
    /// <summary>
    /// 截取全屏（所有显示器）
    /// </summary>
    public BitmapSource CaptureFullScreen()
    {
        int left = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        int top = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);

        return CaptureRegion(left, top, width, height);
    }

    /// <summary>
    /// 截取主屏幕
    /// </summary>
    public BitmapSource CapturePrimaryScreen()
    {
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        return CaptureRegion(0, 0, width, height);
    }

    /// <summary>
    /// 截取指定区域
    /// </summary>
    public BitmapSource CaptureRegion(int x, int y, int width, int height)
    {
        IntPtr desktopWnd = NativeMethods.GetDesktopWindow();
        IntPtr desktopDC = NativeMethods.GetWindowDC(desktopWnd);
        IntPtr memDC = NativeMethods.CreateCompatibleDC(desktopDC);
        IntPtr bitmap = NativeMethods.CreateCompatibleBitmap(desktopDC, width, height);
        IntPtr oldBitmap = NativeMethods.SelectObject(memDC, bitmap);

        NativeMethods.BitBlt(memDC, 0, 0, width, height, desktopDC, x, y, NativeMethods.SRCCOPY);
        NativeMethods.SelectObject(memDC, oldBitmap);

        BitmapSource bitmapSource;
        using (var bmp = System.Drawing.Image.FromHbitmap(bitmap))
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            bitmapSource = bi;
        }

        NativeMethods.DeleteObject(bitmap);
        NativeMethods.DeleteDC(memDC);
        NativeMethods.ReleaseDC(desktopWnd, desktopDC);

        return bitmapSource;
    }

    /// <summary>
    /// 截取指定窗口
    /// </summary>
    public BitmapSource? CaptureWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return null;
        if (!NativeMethods.GetWindowRect(hWnd, out var rect)) return null;

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0) return null;

        return CaptureRegion(rect.Left, rect.Top, width, height);
    }

    /// <summary>
    /// 获取鼠标所在窗口
    /// </summary>
    public IntPtr GetWindowUnderCursor()
    {
        NativeMethods.GetCursorPos(out var point);
        return NativeMethods.WindowFromPoint(point);
    }
}
