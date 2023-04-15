using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
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
    /// Interaction logic for TextGrid.xaml
    /// </summary>
    public partial class TextGrid : UserControl
    {
        public SpecifiedGridObservableCollection<string> SpecifiedGrid
        {
            get { return (SpecifiedGridObservableCollection<string>)GetValue(SpecifiedGridProperty); }
            set { SetValue(SpecifiedGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeadersProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecifiedGridProperty =
            DependencyProperty.Register("SpecifiedGrid", typeof(SpecifiedGridObservableCollection<string>), typeof(TextGrid), new PropertyMetadata(null, new PropertyChangedCallback(OnSpecifiedGridNewObject)));

        public TextGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IMPORTANT: for this to trigger ColumnHeaders or RowsSource needs to be assigned a new object
        ///            something like rows.Add(o) will NOT trigger only  rows = new ObservableCollection<>...
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSpecifiedGridNewObject(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextGrid c = (TextGrid)d;
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
                c.MainGrid.Children.Add(c.NewEntry(0, i, headers[i]));
            }


            for (int row = 0; row < c.SpecifiedGrid.Count; row++)
            {
                c.MainGrid.RowDefinitions.Add(new RowDefinition());
                for (int column = 0; column < headers.Length; column++)
                {

                    c.MainGrid.Children.Add(c.NewEntry(row + 1, column, c.SpecifiedGrid.GetRow(row)[column]));
                }
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
                var tuple = (Tuple<string[], string>)e.NewItems[0];
                string[] newRow = tuple.Item1;

                MainGrid.RowDefinitions.Add(new RowDefinition());

                for (int i = 0; i < newRow.Length; i++)
                {
                    MainGrid.Children.Add(NewEntry(e.NewStartingIndex + 1, i, newRow[i]));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                MainGrid.Children.RemoveRange(SpecifiedGrid.GetHeaders().Length, MainGrid.Children.Count - SpecifiedGrid.GetHeaders().Length);
                MainGrid.RowDefinitions.Clear();
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }

        }


        private UIElement NewEntry(int row, int column, string text)
        {
            Border b = new Border();
            b.Child = new TextBox { Text = text };
            Grid.SetColumn(b, column);
            Grid.SetRow(b, row);
            return b;
        }

    }
}
