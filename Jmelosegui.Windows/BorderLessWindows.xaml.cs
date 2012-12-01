using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Jmelosegui.Windows.Native;
using Application = System.Windows.Forms.Application;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;

namespace Jmelosegui.Windows
{
    public class BorderlessWindow : Window
    {
        private Grid frameGrid;

        public BorderlessWindow()
        {
            Initialized += BorderlessWindow_Initialized;
        }

        void BorderlessWindow_Initialized(object sender, EventArgs ea)
        {
            UpdateDwmBorder();
            Activated += (EventHandler)((s, e) => UpdateDwmBorder());
            Deactivated += (EventHandler)((s, e) => UpdateDwmBorder());

            var behavior = new BorderlessWindowBehavior();
            Interaction.GetBehaviors(this).Add(behavior);
            PreviewMouseMove += HandlePreviewMouseMove;
            PreviewMouseDown += HandleHeaderPreviewMouseDown;
        }

        protected Button MaximizeButton { get; private set; }
        protected Button RestoreButton { get; private set; }
        protected Button CloseButton { get; private set; }
        protected Button MinimizeButton { get; private set; }

        private Border noDwmBorder;
        private Rectangle top;
        private Rectangle bottom;
        private Rectangle left;
        private Rectangle right;
        private Rectangle bottomLeft;
        private Rectangle bottomRight;
        private Rectangle topRight;
        private Rectangle topLeft;

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

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

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
                    UnsafeNativeMethods.SystemParametersInfo(SystemParametersInfoAction.SpiGetdropshadow, 0U,
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
            if (!doResize)
                return;
            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            if (hwndSource == null)
                return;
            SendMessage(hwndSource.Handle, 274U, (IntPtr)((long)(61440 + index)), IntPtr.Zero);
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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
            Resize(sender as Rectangle, true);
        }

        [DebuggerStepThrough]
        private void WindowResized(bool maximized)
        {
            MaximizeButton.Visibility = maximized ? Visibility.Collapsed : Visibility.Visible;
            RestoreButton.Visibility = maximized ? Visibility.Visible : Visibility.Collapsed;
            frameGrid.IsHitTestVisible = !maximized;
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

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            CreateChromeButtons(newContent);
            CreateBorders(newContent);
        }

        private void CreateChromeButtons(object newContent)
        {
            var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                };

            var path = new Path
                {
                    Margin = new Thickness(12, 15, 12, 7),
                    Data = Geometry.Parse("M0,0L0,2 7.999,2 7.999,0 0,0z"),
                };
            path.SetBinding(ForegroundProperty, "{Binding Path=Foreground, RelativeSource={RelativeSource AncestorType=Button}}");
            MinimizeButton = CreateChromeButton("minimizar", path);
            MinimizeButton.Click += Minimize;
            panel.Children.Add(MinimizeButton);

            path = new Path
                {
                    Margin = new Thickness(12, 10, 12, 10),
                    Data = Geometry.Parse("M0,9.999L0,0 9.998,0 9.998,9.999 0,9.999z M8.998,3L1,3 1,8.999 8.999,8.999 8.999,3z"),
                };

            MaximizeButton = CreateChromeButton("maximizar", path);
            MaximizeButton.Click += Restore;
            panel.Children.Add(MaximizeButton);


            path = new Path
                {
                    Margin = new Thickness(12, 10, 12, 10),
                    Data =
                        Geometry.Parse(
                            "M8,6.999L8,9.999 0,9.999 0,3 2,3 2,0 9.999,0 9.999,6.999 8,6.999z M1,8.999L7,8.999 7,4.999 1,4.999 1,8.999z M8.999,2L3,2 3,3 8,3 8,5.999 8.999,5.999 8.999,2z"),
                };
            path.SetBinding(ForegroundProperty, "{Binding Path=Foreground, RelativeSource={RelativeSource AncestorType=Button}}");
            RestoreButton = CreateChromeButton("restaurar", path);
            RestoreButton.Click += Restore;
            RestoreButton.Visibility = Visibility.Collapsed;
            panel.Children.Add(RestoreButton);

            path = new Path
                {
                    Margin = new Thickness(12, 10, 12, 10),
                    Data =
                        Geometry.Parse(
                            "M10.009,1.704L8.331,0.026 5.03,3.327 1.703,0 0,1.704 3.326,5.031 0.025,8.332 1.703,10.009 5.004,6.708 8.305,10.009 10.009,8.305 6.708,5.005"),
                };
            path.SetBinding(ForegroundProperty, "{Binding Path=Foreground, RelativeSource={RelativeSource AncestorType=Button}}");
            CloseButton = CreateChromeButton("cerrar", path);
            CloseButton.Click += Close;
            panel.Children.Add(CloseButton);

            ((IAddChild)newContent).AddChild(panel);
        }

        private void CreateBorders(object newContent)
        {
            frameGrid = new Grid { Name = "frameGrid" };

            noDwmBorder = new Border
                {
                    Name = "noDwmBorder",
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF9900")),
                    Visibility = Visibility.Hidden,
                    IsHitTestVisible = false
                };

            frameGrid.Children.Add(noDwmBorder);

            top = CreateChromeRectangle("top", new Thickness(8, 0, 8, 0), null, VerticalAlignment.Top, null, 7);
            frameGrid.Children.Add(top);

            bottom = CreateChromeRectangle("Bottom", new Thickness(8, 0, 8, 0), null, VerticalAlignment.Bottom, null, 7);
            frameGrid.Children.Add(bottom);

            left = CreateChromeRectangle("left", new Thickness(0, 7, 0, 7), HorizontalAlignment.Left, null, 8, null);
            frameGrid.Children.Add(left);

            right = CreateChromeRectangle("right", new Thickness(0, 7, 0, 7), HorizontalAlignment.Right, null, 8, null);
            frameGrid.Children.Add(right);

            bottomLeft = CreateChromeRectangle("bottomLeft", null, HorizontalAlignment.Left, VerticalAlignment.Bottom, 8, 7);
            frameGrid.Children.Add(bottomLeft);

            bottomRight = CreateChromeRectangle("bottomRight", null, HorizontalAlignment.Right, VerticalAlignment.Bottom, 8, 7);
            frameGrid.Children.Add(bottomRight);

            topRight = CreateChromeRectangle("topRight", null, HorizontalAlignment.Right, VerticalAlignment.Top, 8, 7);
            frameGrid.Children.Add(topRight);

            topLeft = CreateChromeRectangle("topLeft", null, HorizontalAlignment.Left, VerticalAlignment.Top, 8, 7);
            frameGrid.Children.Add(topLeft);

            ((IAddChild)newContent).AddChild(frameGrid);
        }

        private Rectangle CreateChromeRectangle(string name, Thickness? marging, HorizontalAlignment? horizontalAlignment, VerticalAlignment? verticalAlignment, double? width, double? heigth)
        {
            var result = new Rectangle();
            
            result.Name = name;
            
            if (horizontalAlignment.HasValue)
                result.HorizontalAlignment = horizontalAlignment.Value;
            
            if (verticalAlignment.HasValue)
                VerticalAlignment = verticalAlignment.Value;
            
            if (width.HasValue)
                result.Width = width.Value;

            if (heigth.HasValue)
                result.Width = heigth.Value;

            if (marging.HasValue)
            result.Margin = marging.Value;
            //result.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000")),
            //result.Stroke = null,

            result.PreviewMouseDown += HandleRectanglePreviewMouseDown;
            result.PreviewMouseDown += HandleRectangleMouseMove;

            return result;
        }

        private Button CreateChromeButton(string toolTip, Path content)
        {

            var bin = new Binding("Foreground");
            bin.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1);
            content.SetBinding(Shape.FillProperty, bin);

            var result = new Button
                {
                    Content = content,
                    IsTabStop = false,
                    ToolTip = toolTip,
                    Style = GetChromeButtonStyle(),
                };
            //MaximizeButton.RenderOptions.EdgeMode = "Aliased";
            return result;
        }

        /// <summary>
        /// Override this method if you want to set custom styles to "chrome buttons"
        /// </summary>
        /// <returns></returns>
        protected virtual Style GetChromeButtonStyle()
        {
            var chromeButtonStyle = new ResourceDictionary
            {
                Source = new Uri("/Jmelosegui.Windows;component/ChromeButtons.xaml", UriKind.RelativeOrAbsolute)
            };

            return chromeButtonStyle["ChromeButtonStyle"] as Style;
        }
    }
}