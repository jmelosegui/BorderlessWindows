using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Shapes;
using Jmelosegui.Windows.Native;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Controls.Button;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Jmelosegui.Windows
{
    //[TemplatePart(Name = "PART_MINIMIZE", Type = typeof(Button))]
    public class BorderlessWindow : Window
    {
        public BorderlessWindow()
        {
            Style = GetWindowStyle();
            CreateCommandBindings();
        }


        private Grid FrameGrid { get; set; }

        private Border noDwmBorder;
        private Rectangle top;
        private Rectangle bottom;
        private Rectangle left;
        private Rectangle right;
        private Rectangle bottomLeft;
        private Rectangle bottomRight;
        private Rectangle topRight;
        private Rectangle topLeft;

        private Button MinimizeButton { get; set; }
        private Button MaximizeButton { get; set; }
        private Button RestoreButton { get; set; }
        private Button CloseButton { get; set; }

        private readonly RoutedUICommand minimizeCommand = new RoutedUICommand("Minimize", "Minimize", typeof(BorderlessWindow));

        private void AttachMinimizeButton()
        {
            if (MinimizeButton != null)
            {
                MinimizeButton.Command = null;
            }

            var minimizeButton = GetChildControl<Button>("PART_MINIMIZE_BUTTON");
            if (minimizeButton != null)
            {
                minimizeButton.Command = minimizeCommand;
                MinimizeButton = minimizeButton;
            }

            if (this.ResizeMode == ResizeMode.NoResize)
            {
                MinimizeButton.Visibility = Visibility.Hidden;
            }
        }

        private readonly RoutedUICommand maximizeCommand = new RoutedUICommand("Maximize", "Maximize", typeof(BorderlessWindow));

        private void AttachMaximizeButton()
        {
            if (MaximizeButton != null)
            {
                MaximizeButton.Command = null;
            }

            var maximizeButton = GetChildControl<Button>("PART_MAXIMIZE_BUTTON");
            if (maximizeButton != null)
            {
                maximizeButton.Command = maximizeCommand;
                MaximizeButton = maximizeButton;
            }

            if (this.ResizeMode == ResizeMode.NoResize)
            {
                MaximizeButton.Visibility = Visibility.Hidden;
            }
            if (this.ResizeMode == ResizeMode.CanMinimize)
            {
                MaximizeButton.IsEnabled = false;
            }
        }

        private readonly RoutedUICommand restoreCommand = new RoutedUICommand("Restore", "Restore", typeof(BorderlessWindow));

        private void AttachRestoreButton()
        {
            if (RestoreButton != null)
            {
                RestoreButton.Command = null;
            }

            var restoreButton = GetChildControl<Button>("PART_RESTORE_BUTTON");
            if (restoreButton != null)
            {
                restoreButton.Command = restoreCommand;
                RestoreButton = restoreButton;
            }
        }
        
        private readonly RoutedUICommand closeCommand = new RoutedUICommand("Close", "Close", typeof(BorderlessWindow));

        private void AttachCloseButton()
        {
            if (CloseButton != null)
            {
                CloseButton.Command = null;
            }

            var closeButton = GetChildControl<Button>("PART_CLOSE_BUTTON");
            if (closeButton != null)
            {
                closeButton.Command = closeCommand;
                CloseButton = closeButton;
            }
        }
        
        private void AttachFrameGrid()
        {
            var frameGrid = GetChildControl<Grid>("PART_FRAMEGRID");
            if (frameGrid != null)
            {
                FrameGrid = frameGrid;
            }

            var border = GetChildControl<Border>("PART_NODWMBORDER");
            if (border != null)
            {
                noDwmBorder = border;
            }

            top = CreateRectangle(top, "PART_TOP_BORDER");
            bottom = CreateRectangle(bottom, "PART_BOTTOM_BORDER");
            left = CreateRectangle(left, "PART_LEFT_BORDER");
            right = CreateRectangle(right, "PART_RIGHT_BORDER");
            bottomLeft = CreateRectangle(bottomLeft, "PART_BOTTOMLEFT_BORDER");
            bottomRight = CreateRectangle(bottomRight, "PART_BOTTOMRIGHT_BORDER");
            topRight = CreateRectangle(topRight, "PART_TOPRIGHT_BORDER");
            topLeft = CreateRectangle(topLeft, "PART_TOPLEFT_BORDER");
        }

        private Rectangle CreateRectangle(Rectangle rectangle, string dpName)
        {
            if (rectangle != null)
            {
                rectangle.PreviewMouseDown -= HandleRectanglePreviewMouseDown;
                rectangle.MouseMove -= HandleRectangleMouseMove;
            }

            var newRectangle = GetChildControl<Rectangle>(dpName);
            if (newRectangle != null)
            {
                rectangle = newRectangle;
                rectangle.PreviewMouseDown += HandleRectanglePreviewMouseDown;
                rectangle.MouseMove += HandleRectangleMouseMove;
            }

            return rectangle;
        }

        private T GetChildControl<T>(string controlName) where T : DependencyObject
        {
            return GetTemplateChild(controlName) as T;
        }        
        

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();

            UpdateDwmBorder();
            Activated += (EventHandler)((s, e) => UpdateDwmBorder());
            Deactivated += (EventHandler)((s, e) => UpdateDwmBorder());

            var behavior = new BorderlessWindowBehavior();
            Interaction.GetBehaviors(this).Add(behavior);
            PreviewMouseMove += HandlePreviewMouseMove;
            PreviewMouseDown += HandleHeaderPreviewMouseDown;
        }

        private void AttachToVisualTree()
        {
            AttachCloseButton();
            AttachMinimizeButton();
            AttachMaximizeButton();
            AttachRestoreButton();
            AttachFrameGrid();
        }

        private void CreateCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(closeCommand, (a, b) => Close()));
         
            CommandBindings.Add(new CommandBinding(minimizeCommand, (a, b) => { WindowState = WindowState.Minimized; }));
            
            CommandBindings.Add(new CommandBinding(maximizeCommand, Restore));

            CommandBindings.Add(new CommandBinding(restoreCommand, Restore));
        }
       
        private readonly Dictionary<ResizeDirection, Cursor> resizeCursors = new Dictionary
            <ResizeDirection, Cursor>
            {
                {ResizeDirection.Top, Cursors.SizeNS},
                {ResizeDirection.Bottom, Cursors.SizeNS},
                {ResizeDirection.Left, Cursors.SizeWE},
                {ResizeDirection.Right, Cursors.SizeWE},
                {ResizeDirection.TopLeft, Cursors.SizeNWSE},
                {ResizeDirection.TopRight, Cursors.SizeNESW},
                {ResizeDirection.BottomLeft, Cursors.SizeNESW},
                {ResizeDirection.BottomRight, Cursors.SizeNWSE}
            };



        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        private void UpdateDwmBorder()
        {
            if (WindowState == WindowState.Maximized)
            {
                noDwmBorder.Visibility = Visibility.Hidden;
            }
            else
            {
                bool flag1 = Environment.OSVersion.Version.Major >= 6 && !HardwareRenderingHelper.IsInSoftwareMode &&
                             DwmIsCompositionEnabled() && !SystemInformation.TerminalServerSession;
                uint pvParam = 0U;
                bool flag2 =
                    UNSAFENATIVEMETHODS.SystemParametersInfo(SystemParametersInfoAction.SpiGetdropshadow, 0U,
                                                             ref pvParam, 0U) && (int)pvParam != 0;
                noDwmBorder.Visibility = ((flag1 && flag2 && !IsActive) ? Visibility.Hidden : Visibility.Visible);
                
            }
        }

        private void Restore(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                noDwmBorder.Visibility = Visibility.Hidden;
                Application.DoEvents();
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Resize(Rectangle borderRectangle, bool doResize = false)
        {            
            if (!doResize)
                return;
            ResizeDirection index;
            if (Equals(borderRectangle, top))
                index = ResizeDirection.Top;
            else if (Equals(borderRectangle, bottom))
                index = ResizeDirection.Bottom;
            else if (Equals(borderRectangle, left))
                index = ResizeDirection.Left;
            else if (Equals(borderRectangle, right))
                index = ResizeDirection.Right;
            else if (Equals(borderRectangle, topLeft))
                index = ResizeDirection.TopLeft;
            else if (Equals(borderRectangle, topRight))
                index = ResizeDirection.TopRight;
            else if (Equals(borderRectangle, bottomLeft))
            {
                index = ResizeDirection.BottomLeft;
            }
            else
            {
                if (!Equals(borderRectangle, bottomRight))
                    return;
                index = ResizeDirection.BottomRight;
            }
            Cursor = resizeCursors[index];

            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            if (hwndSource == null)
                return;
            SendMessage(hwndSource.Handle, 274U, (IntPtr)((long)(61440 + index)), IntPtr.Zero);
        }
        
        [DebuggerStepThrough]
        private void HandleHeaderPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount <= 1 || Mouse.GetPosition(this).Y > 28.0)
                return;
            Restore(sender, e);
        }

        [DebuggerStepThrough]
        private void HandlePreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                return;
            Cursor = Cursors.Arrow;
        }

        [DebuggerStepThrough]
        private void HandleRectangleMouseMove(object sender, MouseEventArgs e)
        {
            Resize(sender as Rectangle);
        }

        [DebuggerStepThrough]
        private void HandleRectanglePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Resize(sender as Rectangle, this.ResizeMode != System.Windows.ResizeMode.NoResize);
        }

        [DebuggerStepThrough]
        private void WindowResized(bool maximized)
        {
            MaximizeButton.Visibility = maximized ? Visibility.Hidden : Visibility.Visible;
            RestoreButton.Visibility = maximized ? Visibility.Visible : Visibility.Hidden;
            FrameGrid.IsHitTestVisible = !maximized;
            UpdateDwmBorder();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            WindowResized(WindowState == WindowState.Maximized);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.RestorePlacement();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.SavePlacement();
            base.OnClosing(e);
        }

        private Style GetWindowStyle()
        {
            var chromeButtonStyle = new ResourceDictionary
            {
                Source = new Uri("/Jmelosegui.Windows;component/ChromeButtons.xaml", UriKind.RelativeOrAbsolute)
            };

            return chromeButtonStyle["BorderlessWindow"] as Style;
        }
    }
}