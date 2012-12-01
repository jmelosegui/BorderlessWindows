using System;
using System.Windows.Interop;
using System.Windows.Media;

namespace Jmelosegui.Windows
{
    public static class HardwareRenderingHelper
    {
        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static bool IsInSoftwareMode { get; private set; }

        public static void DisableHwRenderingForCrapVideoCards()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GH_FORCE_HW_RENDERING")))
                return;
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GH_FORCE_SW_RENDERING")))
                EnableSoftwareMode();
            else if (Environment.OSVersion.Version.Major * 100 + Environment.OSVersion.Version.Minor < 601)
            {
                //HardwareRenderingHelper.log.Warn("Hardware acceleration is much more glitchy on OS's earlier than Vista");
                //HardwareRenderingHelper.log.Warn("If you believe this isn't the case, set the GH_FORCE_HW_RENDERING environment variable");
                EnableSoftwareMode();
            }
            else
            {
                //HardwareRenderingHelper.log.Info("Your video card appears to support hardware rendering. If this isn't the case and you see glitches");
                //HardwareRenderingHelper.log.Info("set the GH_FORCE_SW_RENDERING environment variable to 1");
            }
        }

        private static void EnableSoftwareMode()
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            IsInSoftwareMode = true;
        }
    }
}
