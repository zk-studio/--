using System.Windows;
using ScreenshotTranslator.Views;

namespace ScreenshotTranslator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化热键服务
        App.HotkeyService.Initialize(this);
        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        var settings = App.SettingsService.Settings;

        App.HotkeyService.RegisterHotkey(settings.RegionCaptureHotkey, () =>
            Dispatcher.Invoke(() => StartRegionCapture()));

        App.HotkeyService.RegisterHotkey(settings.FullScreenCaptureHotkey, () =>
            Dispatcher.Invoke(() => StartFullScreenCapture()));

        App.HotkeyService.RegisterHotkey(settings.TranslateCaptureHotkey, () =>
            Dispatcher.Invoke(() => StartTranslateCapture()));

        App.HotkeyService.RegisterHotkey(settings.OcrCaptureHotkey, () =>
            Dispatcher.Invoke(() => StartOcrCapture()));
    }

    private void StartRegionCapture()
    {
        var captureWindow = new CaptureWindow(CaptureMode.Region);
        captureWindow.Show();
    }

    private void StartFullScreenCapture()
    {
        var bitmap = App.CaptureService.CaptureFullScreen();
        App.ClipboardService.CopyImage(bitmap);

        if (App.SettingsService.Settings.AutoSave)
        {
            var path = App.SettingsService.GenerateScreenshotPath();
            Helpers.ImageHelper.SaveBitmapSource(bitmap, path);
        }

        // 打开编辑窗口
        var editor = new EditorWindow(bitmap);
        editor.Show();
    }

    private void StartTranslateCapture()
    {
        var captureWindow = new CaptureWindow(CaptureMode.Translate);
        captureWindow.Show();
    }

    private void StartOcrCapture()
    {
        var captureWindow = new CaptureWindow(CaptureMode.Ocr);
        captureWindow.Show();
    }

    // ===== 托盘菜单事件 =====

    private void RegionCapture_Click(object sender, RoutedEventArgs e) => StartRegionCapture();
    private void FullScreenCapture_Click(object sender, RoutedEventArgs e) => StartFullScreenCapture();
    private void TranslateCapture_Click(object sender, RoutedEventArgs e) => StartTranslateCapture();
    private void OcrCapture_Click(object sender, RoutedEventArgs e) => StartOcrCapture();

    private void History_Click(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow();
        historyWindow.Show();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        var hasUpdate = await App.UpdateService.CheckForUpdateAsync();
        if (!hasUpdate)
        {
            MessageBox.Show("当前已是最新版本！", "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var result = MessageBox.Show("发现新版本，是否立即更新？", "检查更新",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await App.UpdateService.DownloadAndApplyAsync();
                App.UpdateService.ApplyAndRestart();
            }
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var version = App.UpdateService.GetCurrentVersion();
        MessageBox.Show(
            $"截图翻译工具 v{version}\n\n" +
            "功能：截图、标注、OCR、翻译\n" +
            "翻译引擎：MyMemory（免费）\n" +
            "OCR 引擎：Windows 内置 OCR\n\n" +
            "开源地址：GitHub",
            "关于", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        TrayIcon.Dispose();
        Application.Current.Shutdown();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (App.SettingsService.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            TrayIcon.Dispose();
            base.OnClosing(e);
        }
    }
}
