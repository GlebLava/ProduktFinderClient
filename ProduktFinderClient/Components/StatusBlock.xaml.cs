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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProduktFinderClient.Components
{
    /// <summary>
    /// Interaction logic for StatusBlock.xaml
    /// </summary>
    public partial class StatusBlock : UserControl
    {
        private static readonly int MAX_STATUSES = 100;

        private readonly List<StatusHandle> statusHandles = new();

        public StatusBlock()
        {
            InitializeComponent();
        }

        public StatusHandle AddNewStatus()
        {

            double width = this.ActualWidth / 2.0;

            StackPanel statusPanel = new() { Orientation = Orientation.Horizontal };

            TextBox tLeft = new TextBox();
            TextBox tRight = new TextBox();

            statusPanel.Children.Add(tLeft);
            statusPanel.Children.Add(tRight);

            //Datacontext of the textBoxes needs to be set or else the grid wont get updated when text in one of them changes
            tLeft.DataContext = this;
            tRight.DataContext = this;

            tLeft.Text = "";
            tRight.Text = "";

            MainStackPanel.Children.Add(statusPanel);
            StatusHandle statusHandle = new(tLeft, tRight);
            statusHandles.Add(statusHandle);

            if (!MainScrollViewer.IsFocused)
                MainScrollViewer.ScrollToBottom();

            if (MainStackPanel.Children.Count > MAX_STATUSES)
                MainStackPanel.Children.RemoveAt(0);

            return statusHandle;
        }
    }

    public class StatusHandle
    {
        public TextBox TextBoxLeft { get; private set; }
        public string TextLeft
        {
            get { return TextBoxLeft.Text; }
            set { TextBoxLeft.Text = value; }
        }
        public Color ColorLeft
        {
            set { TextBoxLeft.Foreground = new SolidColorBrush(value); }
        }


        public TextBox TextBoxRight { get; private set; }
        public string TextRight
        {
            get { return TextBoxRight.Text; }
            set { TextBoxRight.Text = value; }
        }

        public Color ColorRight
        {
            set { TextBoxRight.Foreground = new SolidColorBrush(value); }
        }

        public StatusHandle(TextBox textBoxLeft, TextBox textBoxRight)
        {
            this.TextBoxLeft = textBoxLeft;
            this.TextBoxRight = textBoxRight;
        }

    }
}
