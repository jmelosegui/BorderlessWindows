using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
            try
            {
                WriteToFile(GetPlacement(GetHandle(window)));
            }
            catch
            {
            }
        }

        public static void RestorePlacement(this Window window)
        {
            var placement = new WINDOWPLACEMENT();
            try
            {
                var readFromFile = ReadFromFile();

                if (readFromFile.HasValue) 
                    placement = readFromFile.Value;
            }
            catch
            {
                placement = new WINDOWPLACEMENT { Length = -1 };
            }
            IntPtr handle = window.GetHandle();

            if (placement.Length != -1)
                SetPlacement(handle, placement);
            SizeWindowToScreen(window, handle);
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

        private static WINDOWPLACEMENT GetPlacement(IntPtr windowHandle)
        {
            WINDOWPLACEMENT lpwndpl;
            UNSAFENATIVEMETHODS.GetWindowPlacement(windowHandle, out lpwndpl);
            return lpwndpl;
        }

        private static void SetPlacement(IntPtr windowHandle, WINDOWPLACEMENT placement)
        {
            placement.Length = Marshal.SizeOf(typeof (WINDOWPLACEMENT));
            placement.Flags = 0;
            placement.ShowCmd = placement.ShowCmd == SwShowminimized ? SwShownormal : placement.ShowCmd;
            UNSAFENATIVEMETHODS.SetWindowPlacement(windowHandle, ref placement);
        }

        private static WINDOWPLACEMENT? ReadFromFile()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (var stream = new IsolatedStorageFileStream("BorderLessWindowsPlacement.txt", FileMode.Open, isoStore))
            {
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(stream) as WINDOWPLACEMENT?;
            }
        }

        private static void WriteToFile(WINDOWPLACEMENT value)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (var stream =new IsolatedStorageFileStream("BorderLessWindowsPlacement.txt", FileMode.OpenOrCreate, isoStore))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, value);
            }
        }
    }
}