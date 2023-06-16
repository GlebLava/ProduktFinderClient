using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ProduktFinderClient.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private BorderFixComponent borderFixComponent;
        private GlobalFontSizeComponent globalFontSizeComponent;


        private void IntValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Only allow numbers
            int result;
            bool success = int.TryParse(e.Text, out result);

            e.Handled = !success; // e.Handled = true blocks e.Handled = false lets the input through
        }

        private void IntResultTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string updatedText = textBox.Text;

            if (updatedText != "0")
            {
                updatedText = updatedText.TrimStart('0');
                textBox.Text = updatedText;
            }

            if (updatedText == "")
                textBox.Text = "0";
        }

        private void FloatValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string wholeText = textBox.Text + e.Text;

            e.Handled = !float.TryParse(wholeText, out float f);
        }

        private void FloatResultTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string updatedText = textBox.Text;

            if (updatedText == "")
                textBox.Text = "0.0";
        }




        private void SearchResultsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string updatedText = textBox.Text;

            if (updatedText != "0")
            {
                updatedText = updatedText.TrimStart('0');
                textBox.Text = updatedText;
            }

            if (updatedText == "")
                return;


            int parsed = int.Parse(updatedText);
            if (parsed <= 0)
                textBox.Text = "1";
            else if (parsed > 50)
                textBox.Text = "50";
        }

        public OptionsWindow()
        {
            InitializeComponent();
            borderFixComponent = new(this);
            globalFontSizeComponent = new(this);

            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximzeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            App.MainWindowCloseEvent += (s, e) => { Close(); };
        }
    }
}
