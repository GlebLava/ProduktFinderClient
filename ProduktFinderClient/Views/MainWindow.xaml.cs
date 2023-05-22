using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProduktFinderClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region borderFix
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion
        bool mouseOnTools = false;

        public static readonly DependencyProperty DependencyPropertyInfoBoxHeight =
            DependencyProperty.Register(
                name: "InfoBoxHeight",
                propertyType: typeof(double),
                ownerType: typeof(MainWindow),
                typeMetadata: new PropertyMetadata(default(double))
                );

        public double InfoBoxHeight
        {
            get => (double)GetValue(DependencyPropertyInfoBoxHeight);
            set => SetValue(DependencyPropertyInfoBoxHeight, value);
        }

        public static readonly DependencyProperty DependencyPropertyInfoBoxWidth =
    DependencyProperty.Register(
        name: "InfoBoxWidth",
        propertyType: typeof(double),
        ownerType: typeof(MainWindow),
        typeMetadata: new PropertyMetadata(default(double))
        );

        public double InfoBoxWidth
        {
            get => (double)GetValue(DependencyPropertyInfoBoxWidth);
            set => SetValue(DependencyPropertyInfoBoxWidth, value);
        }

        public static readonly DependencyProperty DependencyPropertyInfoBoxFontSize =
        DependencyProperty.Register(
        name: "InfoBoxFontSize",
        propertyType: typeof(double),
        ownerType: typeof(MainWindow),
        typeMetadata: new PropertyMetadata(default(double))
        );

        public double InfoBoxFontSize
        {
            get => (double)GetValue(DependencyPropertyInfoBoxFontSize);
            set => SetValue(DependencyPropertyInfoBoxFontSize, value);
        }


        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximzeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            FontSize = 20;
            InfoBoxFontSize = FontSize * 0.65;
            InfoBoxHeight = FontSize * 5;
            InfoBoxWidth = FontSize * 20;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Delta < 0)
                    MainPartsGrid.GridScrollViewer.LineLeft();
                else
                    MainPartsGrid.GridScrollViewer.LineRight();

                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                if (mouseOnTools)
                {
                    if (e.Delta < 0 && FontSize > 2)
                        FontSize--;
                    else if (FontSize < 50)
                        FontSize++;

                    InfoBoxHeight = FontSize * 5;
                    InfoBoxWidth = FontSize * 20;
                    InfoBoxFontSize = FontSize * 0.65;
                }
                else
                {
                    if (e.Delta < 0 && MainPartsGrid.FontSize > 2)
                        MainPartsGrid.FontSize--;
                    else if (MainPartsGrid.FontSize < 50)
                        MainPartsGrid.FontSize++;
                }

                e.Handled = true;
            }
        }

        private void Tools_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseOnTools = true;
        }

        private void Tools_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseOnTools = false;
        }

    }
}