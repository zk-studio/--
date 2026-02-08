using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ScreenshotTranslator.Helpers;

namespace ScreenshotTranslator.Services;

/// <summary>
/// 全局热键服务
/// </summary>
public class HotkeyService : IDisposable
{
    private IntPtr _windowHandle;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _hotkeyActions = new();
    private int _nextId = 1;
    private bool _disposed;

    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(WndProc);
    }

    /// <summary>
    /// 注册热键
    /// </summary>
    public int RegisterHotkey(string hotkeyString, Action callback)
    {
        ParseHotkey(hotkeyString, out uint modifiers, out uint vk);
        int id = _nextId++;

        if (NativeMethods.RegisterHotKey(_windowHandle, id, modifiers, vk))
        {
            _hotkeyActions[id] = callback;
            return id;
        }

        return -1;
    }

    /// <summary>
    /// 取消注册热键
    /// </summary>
    public void UnregisterHotkey(int id)
    {
        if (_hotkeyActions.ContainsKey(id))
        {
            NativeMethods.UnregisterHotKey(_windowHandle, id);
            _hotkeyActions.Remove(id);
        }
    }

    /// <summary>
    /// 取消所有热键
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var id in _hotkeyActions.Keys.ToList())
        {
            NativeMethods.UnregisterHotKey(_windowHandle, id);
        }
        _hotkeyActions.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                action.Invoke();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private void ParseHotkey(string hotkeyString, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        var parts = hotkeyString.Split('+').Select(p => p.Trim().ToLower()).ToArray();
        foreach (var part in parts)
        {
            switch (part)
            {
                case "ctrl" or "control":
                    modifiers |= NativeMethods.MOD_CONTROL;
                    break;
                case "alt":
                    modifiers |= NativeMethods.MOD_ALT;
                    break;
                case "shift":
                    modifiers |= NativeMethods.MOD_SHIFT;
                    break;
                case "win":
                    modifiers |= NativeMethods.MOD_WIN;
                    break;
                default:
                    // 解析按键
                    if (Enum.TryParse<Key>(part, true, out var key))
                    {
                        vk = (uint)KeyInterop.VirtualKeyFromKey(key);
                    }
                    break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }
}
