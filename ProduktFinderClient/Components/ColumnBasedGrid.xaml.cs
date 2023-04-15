using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ProduktFinderClient.Components
{
    /// <summary>
    /// Interaction logic for ColumnBasedGrid.xaml
    /// </summary>
    public partial class ColumnBasedGrid : UserControl
    {

        //I guess dass hier ist nur für xaml da und kann nicht benutzt werden
        public SpecifiedGridObservableCollection<AttributesInfo> SpecifiedGrid
        {
            get { return (SpecifiedGridObservableCollection<AttributesInfo>)GetValue(SpecifiedGridProperty); }
            set { SetValue(SpecifiedGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeadersProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecifiedGridProperty =
            DependencyProperty.Register("SpecifiedGrid", typeof(SpecifiedGridObservableCollection<AttributesInfo>), typeof(ColumnBasedGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnSpecifiedGridNewObject)));






        //Kann anscheinend kein array sein. Selbst einfaches new object binding funktioniert nicht
        public bool[] ColumnsVisible
        {
            get { return (bool[])GetValue(ColumnsVisibleProperty); }
            set { SetValue(ColumnsVisibleProperty, value); }
        }

        public static readonly DependencyProperty ColumnsVisibleProperty =
            DependencyProperty.Register("ColumnsVisible", typeof(bool[]), typeof(ColumnBasedGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnColumnsVisibleChanged)));



        readonly Style headerBorderStyle;
        readonly Style headerTextStyle;
        readonly Style imageBorderStyle;

        readonly Style baseBorderStyle;
        readonly Style baseTextBoxStyle;

        public ColumnBasedGrid()
        {
            InitializeComponent();

            headerBorderStyle = FindResource("HeaderBorder") as Style;
            headerTextStyle = FindResource("HeaderTextBox") as Style;
            imageBorderStyle = FindResource("ImageBorder") as Style;

            baseBorderStyle = FindResource("BaseBorder") as Style;
            baseTextBoxStyle = FindResource("BaseTextBox") as Style;
        }


        private static void OnColumnsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColumnBasedGrid c = (ColumnBasedGrid)d;

            if (c.ColumnsVisible == null)
                return;

            for (int i = 0; i < c.ColumnsVisible.Length; i++)
            {
                ColumnDefinition column = c.MainGrid.ColumnDefinitions[i];
                if (c.ColumnsVisible[i])
                    column.Width = GridLength.Auto;
                else
                    column.Width = new GridLength(0);  //Microsoft macht das in Falle eines collapses intern auch zu 0
            }
        }



        /// <summary>
        /// IMPORTANT: for this to trigger ColumnHeaders or RowsSource needs to be assigned a new object
        ///            something like rows.Add(o) will NOT trigger only  rows = new ObservableCollection<>...
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSpecifiedGridNewObject(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColumnBasedGrid c = (ColumnBasedGrid)d;
            if (c.SpecifiedGrid == null)
                return;

            c.MainGrid.Children.Clear();
            c.MainGrid.RowDefinitions.Clear();
            c.MainGrid.ColumnDefinitions.Clear();

            c.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            string[] headers = c.SpecifiedGrid.GetHeaders();

            for (int i = 0; i < headers.Length; i++)
            {
                c.MainGrid.ColumnDefinitions.Add(new ColumnDefinition());

                Border b = new Border { Style = c.headerBorderStyle };
                b.Child = new TextBox { Style = c.headerTextStyle, Text = headers[i] };

                Grid.SetColumn(b, i);
                Grid.SetRow(b, 0);

                c.MainGrid.Children.Add(b);
            }




            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= c.OnSpecifiedGridChanged;
            }

            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += c.OnSpecifiedGridChanged;
            }
        }

        private void OnSpecifiedGridChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var tuple = (Tuple<string[], AttributesInfo>)e.NewItems[0];

                string[] newRow = tuple.Item1;
                AttributesInfo aInfo = tuple.Item2;

                MainGrid.RowDefinitions.Add(new RowDefinition());


                for (int i = 0; i < newRow.Length; i++)
                {
                    Border b = null;

                    switch (SpecifiedGrid.GetColumnsType(i))
                    {
                        case ColumnType.Text:
                            b = new Border { Style = baseBorderStyle };
                            b.Child = new TextBox { Style = baseTextBoxStyle, Text = newRow[i] };
                            break;
                        case ColumnType.Image:
                            b = new Border { Style = imageBorderStyle };
                            b.Child = GetImageWithStyle(newRow[i]);
                            break;
                        case ColumnType.Hyperlink:
                            b = new Border { Style = baseBorderStyle };
                            b.Child = GetHyperLinkWithStyle(aInfo.hLink, newRow[i]);
                            break;
                    };

                    Grid.SetColumn(b, i);
                    Grid.SetRow(b, e.NewStartingIndex + 1);   // +1 because first row is reserved for headers
                    MainGrid.Children.Add(b);

                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (ColumnsVisible != null)
                    MainGrid.Children.RemoveRange(ColumnsVisible.Length, MainGrid.Children.Count - ColumnsVisible.Length);   //Columns Visible should have same length as the amount of headers anyways
                
                MainGrid.RowDefinitions.Clear();
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }
        }

        //PROBLEM: BitmapImage doesnt download the image from the URL with Farnell
        //SOLUTION: No solution found yet
        private static Image GetImageWithStyle(string URL)
        {
            if (URL == null || URL == "")
            {
                return new Image();
            }

            Image image = new Image();
            Uri uri = new Uri(@URL, UriKind.Absolute);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            image.Source = bitmap;
            image.Height = 64;
            image.Width = 64;
            return image;
        }


        private static Label GetHyperLinkWithStyle(string URL, string text)
        {
            Label linkLabel = new Label();
            Run linkText = new Run(text);
            Hyperlink link = new Hyperlink(linkText);
            
            

            if (URL == null)
            {
                return linkLabel;
            }

            link.NavigateUri = new Uri(URL, UriKind.Absolute);
            link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender, RequestNavigateEventArgs e)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            });

            linkLabel.Content = link;
            return linkLabel;
        }
    }
}
