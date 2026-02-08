using System.IO;
using Newtonsoft.Json;
using ScreenshotTranslator.Models;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 设置管理服务
/// </summary>
public class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ScreenshotTranslator");

    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    private AppSettings? _settings;

    /// <summary>
    /// 获取当前设置
    /// </summary>
    public AppSettings Settings => _settings ??= Load();

    /// <summary>
    /// 加载设置
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }

        // 设置默认保存目录
        if (string.IsNullOrEmpty(_settings.SaveDirectory))
        {
            _settings.SaveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "ScreenshotTranslator");
        }

        return _settings;
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置为默认设置
    /// </summary>
    public void Reset()
    {
        _settings = new AppSettings();
        Save();
    }

    /// <summary>
    /// 确保截图保存目录存在
    /// </summary>
    public void EnsureSaveDirectoryExists()
    {
        if (!Directory.Exists(Settings.SaveDirectory))
            Directory.CreateDirectory(Settings.SaveDirectory);
    }

    /// <summary>
    /// 生成截图文件名
    /// </summary>
    public string GenerateScreenshotPath()
    {
        EnsureSaveDirectoryExists();
        string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.{Settings.SaveFormat}";
        return Path.Combine(Settings.SaveDirectory, filename);
    }
}
