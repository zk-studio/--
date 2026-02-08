using System.Windows;
using System.Windows.Controls;
using ScreenshotTranslator.Services;

namespace ScreenshotTranslator.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var s = App.SettingsService.Settings;

        // 快捷键
        HotkeyRegion.Text = s.RegionCaptureHotkey;
        HotkeyFullScreen.Text = s.FullScreenCaptureHotkey;
        HotkeyTranslate.Text = s.TranslateCaptureHotkey;
        HotkeyOcr.Text = s.OcrCaptureHotkey;

        // 翻译语言
        var languages = TranslationService.GetSupportedLanguages();
        foreach (var lang in languages)
        {
            SourceLangCombo.Items.Add(new ComboBoxItem { Content = lang.Value, Tag = lang.Key });
            if (lang.Key != "auto")
                TargetLangCombo.Items.Add(new ComboBoxItem { Content = lang.Value, Tag = lang.Key });
        }
        SelectComboByTag(SourceLangCombo, s.SourceLanguage);
        SelectComboByTag(TargetLangCombo, s.TargetLanguage);

        // OCR 语言
        var ocrLanguages = OcrService.GetAvailableLanguages();
        foreach (var lang in ocrLanguages)
        {
            OcrLangCombo.Items.Add(new ComboBoxItem { Content = lang, Tag = lang });
        }
        if (OcrLangCombo.Items.Count == 0)
        {
            OcrLangCombo.Items.Add(new ComboBoxItem { Content = "zh-Hans", Tag = "zh-Hans" });
            OcrLangCombo.Items.Add(new ComboBoxItem { Content = "en-US", Tag = "en-US" });
        }
        SelectComboByTag(OcrLangCombo, s.OcrLanguage);

        // 保存设置
        SaveDirText.Text = s.SaveDirectory;
        AutoSaveCheck.IsChecked = s.AutoSave;
        SelectComboByTag(FormatCombo, s.SaveFormat);

        // 常规设置
        StartWithWindowsCheck.IsChecked = s.StartWithWindows;
        MinimizeToTrayCheck.IsChecked = s.MinimizeToTray;
        AutoCheckUpdateCheck.IsChecked = s.AutoCheckUpdate;

        // GitHub 仓库
        GitHubRepoText.Text = s.GitHubRepo;
    }

    private void SelectComboByTag(ComboBox combo, string tag)
    {
        foreach (ComboBoxItem item in combo.Items)
        {
            if (item.Tag?.ToString() == tag)
            {
                combo.SelectedItem = item;
                return;
            }
        }
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }

    private string GetComboTag(ComboBox combo)
    {
        return (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
    }

    private void BrowseSaveDir_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "选择截图保存目录",
            SelectedPath = SaveDirText.Text
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SaveDirText.Text = dialog.SelectedPath;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = App.SettingsService.Settings;

        s.RegionCaptureHotkey = HotkeyRegion.Text;
        s.FullScreenCaptureHotkey = HotkeyFullScreen.Text;
        s.TranslateCaptureHotkey = HotkeyTranslate.Text;
        s.OcrCaptureHotkey = HotkeyOcr.Text;

        s.SourceLanguage = GetComboTag(SourceLangCombo);
        s.TargetLanguage = GetComboTag(TargetLangCombo);
        s.OcrLanguage = GetComboTag(OcrLangCombo);

        s.SaveDirectory = SaveDirText.Text;
        s.SaveFormat = GetComboTag(FormatCombo);
        s.AutoSave = AutoSaveCheck.IsChecked == true;

        s.StartWithWindows = StartWithWindowsCheck.IsChecked == true;
        s.MinimizeToTray = MinimizeToTrayCheck.IsChecked == true;
        s.AutoCheckUpdate = AutoCheckUpdateCheck.IsChecked == true;

        s.GitHubRepo = GitHubRepoText.Text;

        App.SettingsService.Save();

        // 重新注册热键
        App.HotkeyService.UnregisterAll();
        // 热键会在 MainWindow 重新注册

        MessageBox.Show("设置已保存！部分设置需要重启程序生效。", "设置", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要恢复默认设置吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            App.SettingsService.Reset();
            LoadSettings();
        }
    }
}
