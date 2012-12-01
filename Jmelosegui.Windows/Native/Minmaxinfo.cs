﻿using System.Runtime.InteropServices;

namespace Jmelosegui.Windows.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Minmaxinfo
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }
}