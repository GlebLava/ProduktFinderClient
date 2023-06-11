using ProduktFinderClient.CSV;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProduktFinderClient.Views
{
    /// <summary>
    /// Interaction logic for CSVPreviewWindow.xaml
    /// </summary>
    public partial class CSVPreviewWindow : Window
    {
        private BorderFixComponent borderFixComponent;

        public CSVPreviewWindow()
        {
            InitializeComponent();
            borderFixComponent = new(this);

            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximzeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            App.MainWindowCloseEvent += (s, e) => { Close(); };
        }

        private void Button_ClickBedarf(object sender, RoutedEventArgs e)
        {
            bedarf_DropDown.ClosePopup();
        }

        private void Button_Clickh(object sender, RoutedEventArgs e)
        {
            h_DropDown.ClosePopup();
        }

        private void Button_Clickhcs(object sender, RoutedEventArgs e)
        {
            hcs_DropDown.ClosePopup();
        }


        //Always show most right in Textbox
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.Focus();
                box.Select(box.Text.Length, 0);
            }
        }
    }
}
