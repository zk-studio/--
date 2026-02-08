using System.IO;
using Newtonsoft.Json;
using ScreenshotTranslator.Helpers;
using ScreenshotTranslator.Models;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 截图历史记录服务
/// </summary>
public class HistoryService
{
    private static readonly string HistoryDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ScreenshotTranslator", "History");

    private static readonly string HistoryFile = Path.Combine(HistoryDir, "history.json");

    private List<ScreenshotItem> _items = new();
    private readonly SettingsService _settingsService;

    public HistoryService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        EnsureDirectoryExists();
        Load();
    }

    public IReadOnlyList<ScreenshotItem> Items => _items.AsReadOnly();

    /// <summary>
    /// 添加截图记录
    /// </summary>
    public void Add(ScreenshotItem item)
    {
        _items.Insert(0, item);

        // 限制历史记录数量
        int max = _settingsService.Settings.MaxHistoryCount;
        while (_items.Count > max)
        {
            var removed = _items[_items.Count - 1];
            TryDeleteFile(removed.FilePath);
            TryDeleteFile(removed.ThumbnailPath);
            _items.RemoveAt(_items.Count - 1);
        }

        Save();
    }

    /// <summary>
    /// 删除截图记录
    /// </summary>
    public void Remove(string id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            TryDeleteFile(item.FilePath);
            TryDeleteFile(item.ThumbnailPath);
            _items.Remove(item);
            Save();
        }
    }

    /// <summary>
    /// 清空历史记录
    /// </summary>
    public void Clear()
    {
        foreach (var item in _items)
        {
            TryDeleteFile(item.FilePath);
            TryDeleteFile(item.ThumbnailPath);
        }
        _items.Clear();
        Save();
    }

    /// <summary>
    /// 加载历史记录
    /// </summary>
    private void Load()
    {
        try
        {
            if (File.Exists(HistoryFile))
            {
                var json = File.ReadAllText(HistoryFile);
                _items = JsonConvert.DeserializeObject<List<ScreenshotItem>>(json) ?? new();
                // 清除不存在的文件
                _items.RemoveAll(i => !File.Exists(i.FilePath));
            }
        }
        catch
        {
            _items = new();
        }
    }

    /// <summary>
    /// 保存历史记录
    /// </summary>
    private void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
            File.WriteAllText(HistoryFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存历史记录失败: {ex.Message}");
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(HistoryDir))
            Directory.CreateDirectory(HistoryDir);
    }

    private void TryDeleteFile(string? path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}
