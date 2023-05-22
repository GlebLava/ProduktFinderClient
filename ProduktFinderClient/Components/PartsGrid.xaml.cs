using ProduktFinderClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for PartsGrid.xaml
    /// </summary>
    public partial class PartsGrid : UserControl
    {
        public static readonly List<string> COLUMN_TITLES = new() { "Produkt Bild", "Lieferant", "Hersteller", "Hersteller-TeileNr.", "Beschreibung", "Anzahl Verfügbar", "Mengenpreise" };

        readonly Style headerBorderStyle;
        readonly Style headerTextStyle;

        readonly Style baseBorderStyle;
        readonly Style baseTextBoxStyle;

        readonly Style imageBorderStyle;
        public PartsGrid()
        {
            InitializeComponent();
            headerBorderStyle = (Style)FindResource("HeaderBorder");
            headerTextStyle = (Style)FindResource("HeaderTextBox");
            imageBorderStyle = (Style)FindResource("ImageBorder");

            baseBorderStyle = (Style)FindResource("BaseBorder");
            baseTextBoxStyle = (Style)FindResource("BaseTextBox");

            InitColumns();
        }


        public void ChangeColumnsVisibility(bool[] columnsVisible)
        {
            Debug.Assert(columnsVisible is not null);
            Debug.Assert(columnsVisible.Length == COLUMN_TITLES.Count);

            for (int i = 0; i < columnsVisible.Length; i++)
            {
                //Microsoft handles collapses internally the same way 0
                MainGrid.ColumnDefinitions.Insert(i, new ColumnDefinition() { Width = columnsVisible[i] ? GridLength.Auto : new GridLength(0, GridUnitType.Star) });
            }

        }

        public void AddPartRange(List<Part> parts)
        {
            foreach (Part part in parts)
            {
                AddPart(part);
            }
        }

        // Produkt Bild 0; Lieferant 1; Hersteller 2; Hersteller-TeileNr 3; Beschreibung 4; Anzahl Verfügbar 5; MengenPreise 6;
        public void AddPart(Part part)
        {
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddImageToGrid(part.ImageUrl ?? "", 0);
            AddHyperLinkToGrid(part.Supplier ?? "", part.Hyperlink ?? "", 1);
            AddBaseTextBoxToGrid(part.Manufacturer ?? "", 2);
            AddBaseTextBoxToGrid(part.ManufacturerPartNumber ?? "", 3);
            AddBaseTextBoxToGrid(part.Description ?? "", 4);

            int amount = part.AmountInStock ?? -1;
            AddBaseTextBoxToGrid(amount.ToString(), 5);

            AddBaseTextBoxToGrid(ConstructPrices(part.Prices), 6);
        }

        public void Clear()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.Children.Clear();
            InitColumns();
        }

        private void InitColumns()
        {
            MainGrid.RowDefinitions.Add(new RowDefinition());

            foreach (string title in COLUMN_TITLES)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Border border = new() { Style = headerBorderStyle };
                TextBox textBox = new() { Style = headerTextStyle, Text = title };
                border.Child = textBox;

                MainGrid.Children.Add(border);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, MainGrid.ColumnDefinitions.Count - 1);
            }
        }

        private void AddBaseTextBoxToGrid(string text, int column)
        {
            TextBox textBox = new() { Style = baseTextBoxStyle, Text = text };
            AddBorderWithChildToGrid(textBox, baseBorderStyle, column);
        }

        private void AddHyperLinkToGrid(string hyperLinkText, string hyperLink, int column)
        {
            Label linkLabel = new();
            Run linkText = new(hyperLinkText);
            Hyperlink link = new(linkText);

            if (hyperLink != null)
            {
                link.NavigateUri = new Uri(hyperLink, UriKind.Absolute);
                link.RequestNavigate += new RequestNavigateEventHandler(delegate (object sender, RequestNavigateEventArgs e)
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                    e.Handled = true;
                });

                linkLabel.Content = link;
            }

            AddBorderWithChildToGrid(linkLabel, baseBorderStyle, column);
        }

        void AddImageToGrid(string URL, int column)
        {
            Image image = new();

            if (URL is not null && URL != "")
            {
                Uri uri = new(@URL, UriKind.Absolute);

                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                image.Source = bitmap;
                image.Height = 64;
                image.Width = 64;
            }

            AddBorderWithChildToGrid(image, imageBorderStyle, column);
        }

        void AddBorderWithChildToGrid(UIElement elementToPutInBorder, Style borderStyle, int column)
        {
            Border border = new() { Style = borderStyle };
            border.Child = elementToPutInBorder;

            MainGrid.Children.Add(border);
            Grid.SetRow(border, MainGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(border, column);
        }

        static string ConstructPrices(List<Price> prices)
        {
            if (prices == null || prices.Count == 0)
                return "keine Angabe";

            string s = "";

            foreach (Price price in prices)
            {
                if (price.FromAmount == -1 || price.PricePerPiece == -1.0f)
                    continue;

                s += "Ab " + price.FromAmount + " Stück " + price.PricePerPiece + " " + price.Currency + "\n";
            }

            return s;
        }


    }
}
