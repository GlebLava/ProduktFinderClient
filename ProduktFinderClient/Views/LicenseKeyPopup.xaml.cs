using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProduktFinderClient.Views
{
    /// <summary>
    /// Interaction logic for LicenseKeyPopup.xaml
    /// </summary>
    public partial class LicenseKeyPopup : Window
    {
        private BorderFixComponent borderFixComponent;

        public LicenseKeyPopup()
        {
            InitializeComponent();
            borderFixComponent = new(this);


            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximzeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            FontSize = 20;
        }

        private void Anwenden_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
