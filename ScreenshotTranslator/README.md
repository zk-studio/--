# ScreenshotTranslator 截图翻译工具

<p align="center">
  <strong>🖼️ 截图 · ✏️ 标注 · 🔍 OCR · 🌐 翻译</strong>
</p>

一款 Windows 截图翻译工具，支持区域截图、全屏截图、窗口截图、图片标注、文字识别（OCR）以及一键翻译功能。

## ✨ 功能特性

### 📷 截图功能
- **区域截图** - 自由选择截图区域（快捷键 `Ctrl+Shift+A`）
- **全屏截图** - 一键截取整个屏幕（快捷键 `Ctrl+Shift+F`）
- **延时截图** - 支持 3/5/10 秒延时截图
- **窗口截图** - 自动检测窗口边界
- **钉图** - 将截图钉在桌面最前端

### ✏️ 标注功能
- 矩形、椭圆、箭头绘制
- 自由画笔、荧光笔标注
- 文字添加
- 马赛克/模糊处理
- 撤销/重做

### 🔍 OCR 文字识别
- 基于 Windows 内置 OCR 引擎（免费）
- 支持中文、英文、日文等多种语言
- 一键复制识别文字

### 🌐 截图翻译
- OCR 识别 + 自动翻译一体化
- 使用 MyMemory 免费翻译 API
- 支持多种语言互译
- 翻译结果覆盖显示在原图上

### 🔄 自动更新
- 基于 GitHub Releases 自动检查更新
- 增量更新，节省带宽
- 静默后台更新

## 🚀 快速开始

### 下载安装
前往 [Releases](../../releases) 页面下载最新版本。

### 从源码构建
```bash
# 克隆仓库
git clone https://github.com/YOUR_USERNAME/ScreenshotTranslator.git
cd ScreenshotTranslator

# 构建
dotnet build src/ScreenshotTranslator/ScreenshotTranslator.csproj -c Release

# 运行
dotnet run --project src/ScreenshotTranslator/ScreenshotTranslator.csproj
```

## ⌨️ 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Shift+A` | 区域截图 |
| `Ctrl+Shift+F` | 全屏截图 |
| `Ctrl+Shift+T` | 截图翻译 |
| `Ctrl+Shift+O` | OCR 文字识别 |
| `Escape` | 取消截图 |

## 🛠️ 技术栈

- **.NET 8** + **WPF** - Windows 桌面应用框架
- **Windows.Media.Ocr** - 内置 OCR 引擎（免费）
- **MyMemory API** - 免费翻译服务
- **Velopack** - 自动更新框架
- **GitHub Actions** - CI/CD 自动构建发布

## 📄 License

MIT License - 详见 [LICENSE](LICENSE)
