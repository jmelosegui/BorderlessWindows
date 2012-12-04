using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using Jmelosegui.Windows.Native;

namespace Jmelosegui.Windows
{
    /// <summary>
    /// Borderless Window Behavior
    /// </summary>
    public class BorderlessWindowBehavior : Behavior<Window>
    {
        private const int WmGetminmaxinfo = 0x24;
        private const int WmNcactivate = 0x86;
        private const int WmNccalcsize = 0x83;
        private const int WmNcpaint = 0x85;

        public static DependencyProperty ResizeWithGripDependencyProperty = DependencyProperty.Register(
            "ResizeWithGrip", typeof (bool), typeof (BorderlessWindowBehavior), new PropertyMetadata(true));

        private bool isHardwareRenderingEnabled;
        private IntPtr mHwnd;
        private HwndSource mHwndSource;
        private POINT minimumSize;

        /// <summary>
        /// Gets or sets a value indicating whether [resize with grip].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [resize with grip]; otherwise, <c>false</c>.
        /// </value>
        public bool ResizeWithGrip
        {
            get { return (bool) GetValue(ResizeWithGripDependencyProperty); }
            set { SetValue(ResizeWithGripDependencyProperty, value); }
        }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            if (AssociatedObject.IsInitialized)
                AddHwndHook();
            else
                AssociatedObject.SourceInitialized += AssociatedObjectSourceInitialized;

            AssociatedObject.WindowStyle = WindowStyle.None;
            AssociatedObject.ResizeMode = ResizeWithGrip ? ResizeMode.CanResizeWithGrip : ResizeMode.CanResize;

            AssociatedObject.MouseLeftButtonDown += (s, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        AssociatedObject.DragMove();
                    }
                };

            RegisterDepPropCallback(AssociatedObject, FrameworkElement.HeightProperty, OnSizeChange);
            RegisterDepPropCallback(AssociatedObject, FrameworkElement.WidthProperty, OnSizeChange);

            isHardwareRenderingEnabled = (Environment.OSVersion.Version.Major >= 6 &&
                                          !HardwareRenderingHelper.IsInSoftwareMode);

            base.OnAttached();
        }

        public static void RegisterDepPropCallback(DependencyObject owner, DependencyProperty property, EventHandler handler)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(property, owner.GetType());
            dpd.AddValueChanged(owner, handler);
        }

        private void OnSizeChange(object sender, EventArgs e)
        {
            PresentationSource source = PresentationSource.FromVisual(AssociatedObject);
            if (source != null)
            {
                if (source.CompositionTarget != null)
                {
                    Point deviceMinSize =
                        source.CompositionTarget.TransformToDevice.Transform(
                            new Point(AssociatedObject.MinWidth, AssociatedObject.MinHeight));
                    minimumSize = new POINT((int)deviceMinSize.X, (int)deviceMinSize.Y);
                }
            }
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            RemoveHwndHook();
            base.OnDetaching();
        }

        /// <summary>
        /// Adds the HWND hook.
        /// </summary>
        private void AddHwndHook()
        {
            mHwndSource = PresentationSource.FromVisual(AssociatedObject) as HwndSource;
            if (mHwndSource != null) mHwndSource.AddHook(HwndHook);
            mHwnd = new WindowInteropHelper(AssociatedObject).Handle;
        }

        /// <summary>
        /// Removes the HWND hook.
        /// </summary>
        private void RemoveHwndHook()
        {
            AssociatedObject.SourceInitialized -= AssociatedObjectSourceInitialized;
            mHwndSource.RemoveHook(HwndHook);
        }

        /// <summary>
        /// Handles the SourceInitialized event of the AssociatedObject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void AssociatedObjectSourceInitialized(object sender, EventArgs e)
        {
            AddHwndHook();
        }

        /// <summary>
        /// HWNDs the hook.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="message">The message.</param>
        /// <param name="wParam">The w param.</param>
        /// <param name="lParam">The l param.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        /// <returns></returns>
        private IntPtr HwndHook(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr returnval = IntPtr.Zero;

            switch (message)
            {
                case WmNccalcsize:
                    // Hides the border
                    handled = true;
                    break;
                case WmNcpaint:
                    // Works for Windows Vista and higher
                    if (isHardwareRenderingEnabled)
                    {
                        var m = new MARGINS {bottomHeight = 1, leftWidth = 1, rightWidth = 1, topHeight = 1};
                        UNSAFENATIVEMETHODS.DwmExtendFrameIntoClientArea(mHwnd, ref m);
                    }
                    handled = true;
                    break;
                case WmNcactivate:
                    /* As per http://msdn.microsoft.com/en-us/library/ms632633(VS.85).aspx , "-1" lParam does not
                     * repaint the nonclient area to reflect the state change. */
                    returnval = UNSAFENATIVEMETHODS.DefWindowProc(hWnd, message, wParam, new IntPtr(-1));
                    handled = true;
                    break;
                case WmGetminmaxinfo:
                    /* From Lester's Blog (thanks @aeoth):  
                     * http://blogs.msdn.com/b/llobo/archive/2006/08/01/maximizing-window-_2800_with-windowstyle_3d00_none_2900_-considering-taskbar.aspx */
                    UNSAFENATIVEMETHODS.WmGetMinMaxInfo(hWnd, lParam, minimumSize);
                    handled = true;
                    break;
            }

            return returnval;
        }
    }
}