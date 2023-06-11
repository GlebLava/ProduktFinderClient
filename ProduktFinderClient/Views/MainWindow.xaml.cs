using ProduktFinderClient.Views;
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
        private BorderFixComponent borderFixComponent;


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
            borderFixComponent = new(this);


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