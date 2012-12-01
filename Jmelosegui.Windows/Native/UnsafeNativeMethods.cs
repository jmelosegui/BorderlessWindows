using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Jmelosegui.Windows.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static class UnsafeNativeMethods
    {
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        internal static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, POINT minSize)
        {
            var mmi = (Minmaxinfo) Marshal.PtrToStructure(lParam, typeof (Minmaxinfo));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            const int monitorDefaulttonearest = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, monitorDefaulttonearest);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new Monitorinfo();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                mmi.ptMinTrackSize.x = minSize.x;
                mmi.ptMinTrackSize.y = minSize.y;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr DefWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("dwmapi.dll")]
        internal static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [DllImport("user32", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo([In] IntPtr hMonitor, [Out] Monitorinfo lpmi);

        [DllImport("user32.dll", EntryPoint = "SetClassLong")]
        internal static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
        internal static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref Windowplacement lpwndpl);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out Windowplacement lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(SystemParametersInfoAction uiAction, uint uiParam,
                                                       ref uint pvParam, uint fWinIni);
    }

    public enum SystemParametersInfoAction : uint
    {
        SpiGetdropshadow = 4132U,
        SpiSetdropshadow = 4133U,
    }
}