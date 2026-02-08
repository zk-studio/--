using System.Net.Http;
using Newtonsoft.Json.Linq;
using Velopack;
using Velopack.Sources;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 自动更新服务（基于 Velopack + GitHub Releases）
/// </summary>
public class UpdateService
{
    private readonly SettingsService _settingsService;
    private UpdateManager? _updateManager;

    public event Action<string>? UpdateAvailable;
    public event Action<int>? DownloadProgress;
    public event Action? UpdateReady;
    public event Action<string>? UpdateError;

    public UpdateService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// 初始化更新管理器
    /// </summary>
    public void Initialize()
    {
        var repo = _settingsService.Settings.GitHubRepo;
        if (string.IsNullOrEmpty(repo)) return;

        try
        {
            var source = new GithubSource($"https://github.com/{repo}", null, false);
            _updateManager = new UpdateManager(source);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"初始化更新管理器失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    public async Task<bool> CheckForUpdateAsync()
    {
        if (_updateManager == null) return false;

        try
        {
            var updateInfo = await _updateManager.CheckForUpdatesAsync();
            if (updateInfo != null)
            {
                UpdateAvailable?.Invoke(updateInfo.TargetFullRelease.Version.ToString());
                return true;
            }
        }
        catch (Exception ex)
        {
            UpdateError?.Invoke($"检查更新失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// 下载并应用更新
    /// </summary>
    public async Task DownloadAndApplyAsync()
    {
        if (_updateManager == null) return;

        try
        {
            var updateInfo = await _updateManager.CheckForUpdatesAsync();
            if (updateInfo == null) return;

            await _updateManager.DownloadUpdatesAsync(
                updateInfo,
                progress => DownloadProgress?.Invoke(progress));

            UpdateReady?.Invoke();
        }
        catch (Exception ex)
        {
            UpdateError?.Invoke($"下载更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 应用更新并重启
    /// </summary>
    public void ApplyAndRestart()
    {
        try
        {
            _updateManager?.ApplyUpdatesAndRestart(null);
        }
        catch (Exception ex)
        {
            UpdateError?.Invoke($"应用更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前版本
    /// </summary>
    public string GetCurrentVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString(3) ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
}
