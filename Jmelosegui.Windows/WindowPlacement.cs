using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Jmelosegui.Windows.Native;

namespace Jmelosegui.Windows
{
    public static class WindowPlacement
    {
        private const int SwShownormal = 1;
        private const int SwShowminimized = 2;

        public static void SavePlacement(this Window window)
        {
            //try
            //{
            //    ICacheManager userCache = CacheFactory.GetCacheManager("Cache Manager");
            //    userCache.Add("Application:WindowPlacement", GetPlacement(GetHandle(window)));
            //}
            //catch
            //{
            //}
        }

        public static void RestorePlacement(this Window window)
        {
            //ICacheManager userCache = CacheFactory.GetCacheManager("Cache Manager");
            //WINDOWPLACEMENT placement;
            //try
            //{
            //    placement = (WINDOWPLACEMENT)userCache.GetData("Application:WindowPlacement");
            //}
            //catch
            //{
            //    placement = new WINDOWPLACEMENT { length = -1 };
            //}
            //IntPtr handle = window.GetHandle();

            //if (placement.length != -1)
            //    SetPlacement(handle, placement);
            //SizeWindowToScreen(window, handle);
        }

        private static void SizeWindowToScreen(Window window, IntPtr hwnd)
        {
            Screen screen = Screen.FromHandle(hwnd);
            if (screen.WorkingArea.Width >= window.Width && screen.WorkingArea.Height >= window.Height)
                return;
            if (screen.WorkingArea.Width < window.Width)
                window.Width = screen.WorkingArea.Width;
            if (screen.WorkingArea.Height < window.Height)
                window.Height = screen.WorkingArea.Height;
            window.Left = screen.WorkingArea.Left;
            window.Top = screen.WorkingArea.Top;
        }

        private static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        private static Windowplacement GetPlacement(IntPtr windowHandle)
        {
            Windowplacement lpwndpl;
            UnsafeNativeMethods.GetWindowPlacement(windowHandle, out lpwndpl);
            return lpwndpl;
        }

        private static void SetPlacement(IntPtr windowHandle, Windowplacement placement)
        {
            placement.Length = Marshal.SizeOf(typeof (Windowplacement));
            placement.Flags = 0;
            placement.ShowCmd = placement.ShowCmd == SwShowminimized ? SwShownormal : placement.ShowCmd;
            UnsafeNativeMethods.SetWindowPlacement(windowHandle, ref placement);
        }
    }
}