using System.Runtime.InteropServices;

namespace Jmelosegui.Windows.Native
{
    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class Monitorinfo
    {
        /// <summary>
        /// </summary>            
        public int cbSize = Marshal.SizeOf(typeof (Monitorinfo));

        /// <summary>
        /// </summary>            
        public RECT rcMonitor;

        /// <summary>
        /// </summary>            
        public RECT rcWork;

        /// <summary>
        /// </summary>            
        public int dwFlags;
    }
}