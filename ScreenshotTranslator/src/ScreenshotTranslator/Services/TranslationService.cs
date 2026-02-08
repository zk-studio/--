using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;
using ScreenshotTranslator.Models;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 翻译服务 - 使用 MyMemory 免费翻译 API（每天 5000 字免费，无需 Key）
/// </summary>
public class TranslationService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// 翻译文本
    /// </summary>
    public async Task<TranslationResult> TranslateAsync(string text, string sourceLang = "en", string targetLang = "zh")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new TranslationResult
            {
                Success = false,
                ErrorMessage = "翻译文本为空"
            };
        }

        try
        {
            // 自动检测语言
            string langPair = sourceLang == "auto"
                ? $"en|{targetLang}"
                : $"{sourceLang}|{targetLang}";

            // 如果源语言是auto，先尝试判断是否包含中文
            if (sourceLang == "auto")
            {
                bool hasChinese = text.Any(c => c >= 0x4e00 && c <= 0x9fff);
                if (hasChinese && targetLang == "zh")
                {
                    // 中文翻译成英文
                    langPair = "zh|en";
                }
                else if (hasChinese)
                {
                    langPair = $"zh|{targetLang}";
                }
                else
                {
                    langPair = $"en|{targetLang}";
                }
            }

            string encodedText = HttpUtility.UrlEncode(text);
            string url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair={langPair}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            var translatedText = json["responseData"]?["translatedText"]?.ToString();
            var status = json["responseStatus"]?.Value<int>() ?? 0;

            if (status == 200 && !string.IsNullOrEmpty(translatedText))
            {
                return new TranslationResult
                {
                    Success = true,
                    OriginalText = text,
                    TranslatedText = translatedText,
                    SourceLanguage = sourceLang,
                    TargetLanguage = targetLang
                };
            }

            return new TranslationResult
            {
                Success = false,
                OriginalText = text,
                ErrorMessage = $"翻译失败，状态码: {status}"
            };
        }
        catch (Exception ex)
        {
            return new TranslationResult
            {
                Success = false,
                OriginalText = text,
                ErrorMessage = $"翻译出错: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 分段翻译（对于长文本）
    /// </summary>
    public async Task<TranslationResult> TranslateSegmentsAsync(
        List<(string text, double x, double y, double w, double h)> segments,
        string sourceLang = "en", string targetLang = "zh")
    {
        var result = new TranslationResult
        {
            SourceLanguage = sourceLang,
            TargetLanguage = targetLang
        };

        var allOriginal = new List<string>();
        var allTranslated = new List<string>();

        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.text)) continue;

            var translated = await TranslateAsync(segment.text, sourceLang, targetLang);

            var block = new TranslationBlock
            {
                OriginalText = segment.text,
                TranslatedText = translated.Success ? translated.TranslatedText : segment.text,
                X = segment.x,
                Y = segment.y,
                Width = segment.w,
                Height = segment.h
            };

            result.Blocks.Add(block);
            allOriginal.Add(segment.text);
            allTranslated.Add(block.TranslatedText);

            // 避免请求过于频繁
            await Task.Delay(100);
        }

        result.Success = true;
        result.OriginalText = string.Join("\n", allOriginal);
        result.TranslatedText = string.Join("\n", allTranslated);

        return result;
    }

    /// <summary>
    /// 获取支持的语言列表
    /// </summary>
    public static Dictionary<string, string> GetSupportedLanguages()
    {
        return new Dictionary<string, string>
        {
            ["auto"] = "自动检测",
            ["zh"] = "中文",
            ["en"] = "英语",
            ["ja"] = "日语",
            ["ko"] = "韩语",
            ["fr"] = "法语",
            ["de"] = "德语",
            ["es"] = "西班牙语",
            ["pt"] = "葡萄牙语",
            ["ru"] = "俄语",
            ["ar"] = "阿拉伯语",
            ["it"] = "意大利语",
            ["th"] = "泰语",
            ["vi"] = "越南语"
        };
    }
}
