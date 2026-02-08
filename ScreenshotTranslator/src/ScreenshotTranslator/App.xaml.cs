using System.Windows;
using ScreenshotTranslator.Services;
using Velopack;

namespace ScreenshotTranslator;

public partial class App : Application
{
    public static SettingsService SettingsService { get; } = new();
    public static ScreenCaptureService CaptureService { get; } = new();
    public static OcrService OcrService { get; } = new();
    public static TranslationService TranslationService { get; } = new();
    public static ClipboardService ClipboardService { get; } = new();
    public static HotkeyService HotkeyService { get; } = new();
    public static HistoryService HistoryService { get; private set; } = null!;
    public static UpdateService UpdateService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Velopack 初始化
        VelopackApp.Build().Run();

        base.OnStartup(e);

        // 初始化服务
        HistoryService = new HistoryService(SettingsService);
        UpdateService = new UpdateService(SettingsService);
        UpdateService.Initialize();

        // 自动检查更新
        if (SettingsService.Settings.AutoCheckUpdate)
        {
            _ = UpdateService.CheckForUpdateAsync();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        HotkeyService.Dispose();
        SettingsService.Save();
        base.OnExit(e);
    }
}
