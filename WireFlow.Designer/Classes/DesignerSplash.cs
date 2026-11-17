using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace WireFlow
{
    // The splash screen of the Designer's start, centered on the window about to be shown: on the place the window opens at
    // (ConfigSettings.WindowBounds), or, maximized, on the work area of the screen it opens on. WPF's SplashScreen shows its image
    // before the rest of WPF loads, centered on the primary screen, and has no place to set. So its window (of class SplashScreen) is
    // cloaked as it is made, through a hook on this thread, so that the screen never shows it there; once shown it is moved, and
    // then shown. Where Windows places the window (no place kept, as at the first start, or one no longer on any screen), the splash
    // shows where WPF puts it.
    public static class DesignerSplash
    {
        private const string SPLASH_IMAGE = "SplashScreen.png";
        private const string SPLASH_CLASS = "SplashScreen";


        // Shown as the application is made (App's constructor), when the Designer is the program: a script that makes App shows none
        public static void ShowAtStart()
        {
            if (System.Reflection.Assembly.GetEntryAssembly() != typeof(App).Assembly)
                return;
            ConfigSettings config = null;
            try
            {
                config = ConfigSettings.Read();
            }
            catch (Exception)
            {
                // Settings that cannot be read leave the splash where WPF puts it; the window then reports them
            }
            // It closes itself, fading, once the window has loaded
            Show(config, true);
        }



        // Shows the splash, centered on where the window opens with these settings
        public static SplashScreen Show(ConfigSettings config, bool autoClose)
        {
            var splash = new SplashScreen(typeof(App).Assembly, SPLASH_IMAGE);
            RECT? area = null;
            try
            {
                area = WindowArea(config);
            }
            catch (Exception)
            {
                // Where the place cannot be worked out, the splash shows where WPF puts it
            }
            if (area == null)
            {
                splash.Show(autoClose);
                return splash;
            }
            IntPtr window = IntPtr.Zero;
            // Made, and before WPF draws its image into it, the window is cloaked (WM_CREATE: earlier, DWM does not take the attribute)
            HookProc onCreate = (code, wParam, lParam) =>
            {
                try
                {
                    if (code >= 0 && window == IntPtr.Zero)
                    {
                        var message = Marshal.PtrToStructure<CWPSTRUCT>(lParam);
                        if (message.Message == WM_CREATE && IsSplashWindow(message.Window) && Cloak(message.Window, true))
                            window = message.Window;
                    }
                }
                catch (Exception)
                {
                    // An exception must not leave a hook called from Windows; the window is then moved uncloaked
                }
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            };
            IntPtr hook = SetWindowsHookEx(WH_CALLWNDPROC, onCreate, IntPtr.Zero, GetCurrentThreadId());
            try
            {
                splash.Show(autoClose);
            }
            finally
            {
                if (hook != IntPtr.Zero)
                    UnhookWindowsHookEx(hook);
                GC.KeepAlive(onCreate);
            }
            try
            {
                Center(window != IntPtr.Zero ? window : FindSplashWindow(), area.Value);
            }
            catch (Exception)
            {
                // Where the window cannot be moved, the splash stays where WPF put it
            }
            finally
            {
                if (window != IntPtr.Zero)
                    Cloak(window, false);
            }
            return splash;
        }



        // The area, in device pixels, that the splash is centered on: the window's place and size kept in the settings, or, maximized,
        // the work area of the screen it opens on; null where Windows places the window
        private static RECT? WindowArea(ConfigSettings config)
        {
            if (config?.WindowBounds() is not Rect bounds)
                return null;
            // The process is aware of the system's DPI, as WPF makes it, so a device-independent pixel is that many device pixels
            double scale = GetDpiForSystem() / 96.0;
            if (scale <= 0)
                scale = 1;
            var area = new RECT
            {
                Left = (int)Math.Round(bounds.Left * scale),
                Top = (int)Math.Round(bounds.Top * scale),
                Right = (int)Math.Round(bounds.Right * scale),
                Bottom = (int)Math.Round(bounds.Bottom * scale)
            };
            return config.Maximized ? WorkArea(area) : area;
        }



        // Moves the splash window so that its center lies at the center of the area, kept within the work area of the screen it lies on
        private static void Center(IntPtr splash, RECT area)
        {
            if (splash == IntPtr.Zero || !GetWindowRect(splash, out var own))
                return;
            int width = own.Right - own.Left;
            int height = own.Bottom - own.Top;
            int left = (area.Left + area.Right) / 2 - width / 2;
            int top = (area.Top + area.Bottom) / 2 - height / 2;
            var screen = WorkArea(new RECT { Left = left, Top = top, Right = left + width, Bottom = top + height });
            left = Math.Max(screen.Left, Math.Min(left, screen.Right - width));
            top = Math.Max(screen.Top, Math.Min(top, screen.Bottom - height));
            SetWindowPos(splash, IntPtr.Zero, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        }



        // The work area of the screen that the rectangle lies on the most of, or the nearest
        private static RECT WorkArea(RECT rect)
        {
            var monitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);
            var info = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() };
            return monitor != IntPtr.Zero && GetMonitorInfo(monitor, ref info) ? info.Work : rect;
        }



        // Hides the window from the screen, or shows it again, without changing its visibility: DWM does not draw a cloaked window
        private static bool Cloak(IntPtr window, bool cloak)
        {
            int value = cloak ? 1 : 0;
            return DwmSetWindowAttribute(window, DWMWA_CLOAK, ref value, sizeof(int)) == 0;
        }



        private static bool IsSplashWindow(IntPtr window)
        {
            var name = new StringBuilder(64);
            return GetClassName(window, name, name.Capacity) > 0 && name.ToString() == SPLASH_CLASS;
        }



        // WPF's splash window: the visible window of class SplashScreen on this thread
        private static IntPtr FindSplashWindow()
        {
            IntPtr found = IntPtr.Zero;
            EnumThreadWindows(GetCurrentThreadId(), (hwnd, _) =>
            {
                if (IsWindowVisible(hwnd) && IsSplashWindow(hwnd))
                {
                    found = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }



        #region Win32

        private const int WH_CALLWNDPROC = 4;
        private const uint WM_CREATE = 0x0001;
        private const int DWMWA_CLOAK = 13;
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CWPSTRUCT
        {
            public IntPtr LParam;
            public IntPtr WParam;
            public uint Message;
            public IntPtr Window;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int Size;
            public RECT Monitor;
            public RECT Work;
            public uint Flags;
        }

        private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int hookType, HookProc callback, IntPtr module, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint threadId, EnumWindowsProc callback, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int size);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref RECT rect, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        #endregion
    }
}
