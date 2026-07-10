using System.Runtime.InteropServices;
using System.Text;

namespace ScreenAutoClicker;

/// <summary>
/// Global low-level keyboard hook (WH_KEYBOARD_LL) that captures all keystrokes
/// typed on any application. Accumulates characters in a buffer and fires
/// <see cref="CodeCaptured"/> when Enter is pressed.
/// </summary>
class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_QUIT = 0x0012;

    private const int VK_RETURN = 0x0D;
    private const int VK_BACK   = 0x08;

    private Win32KbHook.LowLevelKeyboardProc? _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private Thread? _thread;
    private int _threadId;

    private readonly StringBuilder _buffer = new();
    private readonly object _lock = new();
    private string _lastCapturedCode = "";

    /// <summary>Fired on Enter with the characters accumulated since the last Enter.</summary>
    public event Action<string>? CodeCaptured;

    /// <summary>The most recently captured code (set after every Enter press).</summary>
    public string LastCode { get { lock (_lock) return _lastCapturedCode; } }

    public KeyboardHook()
    {
        // Keep a strong reference to the delegate so it isn't GC-collected
        _proc = HookCallback;

        var ready = new ManualResetEventSlim(false);

        _thread = new Thread(() =>
        {
            _threadId = Win32KbHook.GetCurrentThreadId();

            // For WH_KEYBOARD_LL the hook runs in the installing process,
            // so hMod = handle of the current process module.
            _hookId = Win32KbHook.SetWindowsHookEx(
                WH_KEYBOARD_LL,
                _proc,
                Win32KbHook.GetModuleHandle(null),
                0);

            ready.Set();

            // Message pump — delivers hook notifications to this thread
            while (Win32KbHook.GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                Win32KbHook.TranslateMessage(ref msg);
                Win32KbHook.DispatchMessage(ref msg);
            }

            if (_hookId != IntPtr.Zero)
            {
                Win32KbHook.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        });

        _thread.IsBackground = true;
        _thread.Start();
        ready.Wait();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vk = Marshal.ReadInt32(lParam);

            if (vk == VK_RETURN)
            {
                string captured;
                lock (_lock)
                {
                    captured = _buffer.ToString().Trim();
                    _buffer.Clear();
                    if (captured.Length > 0)
                        _lastCapturedCode = captured;
                }
                if (captured.Length > 0)
                    CodeCaptured?.Invoke(captured);
            }
            else if (vk == VK_BACK)
            {
                lock (_lock)
                {
                    if (_buffer.Length > 0)
                        _buffer.Remove(_buffer.Length - 1, 1);
                }
            }
            else
            {
                char? c = VkToChar(vk);
                if (c.HasValue)
                    lock (_lock)
                        _buffer.Append(c.Value);
            }
        }

        return Win32KbHook.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>Maps a virtual-key code to its ASCII character (digits + letters only).</summary>
    private static char? VkToChar(int vk)
    {
        // Main keyboard digits 0-9
        if (vk >= 0x30 && vk <= 0x39) return (char)vk;
        // Numpad digits 0-9
        if (vk >= 0x60 && vk <= 0x69) return (char)('0' + vk - 0x60);
        // Letters A-Z (returned as uppercase; barcode scanners typically use uppercase)
        if (vk >= 0x41 && vk <= 0x5A) return (char)vk;
        return null;
    }

    public void Dispose()
    {
        if (_threadId != 0)
        {
            Win32KbHook.PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            _thread?.Join(500);
        }
    }
}

internal static class Win32KbHook
{
    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int ptX;
        public int ptY;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpmsg);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();

    [DllImport("user32.dll")]
    public static extern bool PostThreadMessage(int idThread, int Msg, IntPtr wParam, IntPtr lParam);
}
