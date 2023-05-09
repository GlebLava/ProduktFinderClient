using ProduktFinderClient.DataTypes;
using ProduktFinderClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProduktFinderClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        public static MainWindow mainWindow;

        public static readonly ColumnTypeDefinition[] columnDefinitions = {
            new ColumnTypeDefinition("Produkt Bild", ColumnType.Image),
            new ColumnTypeDefinition("Lieferant", ColumnType.Hyperlink),
            new ColumnTypeDefinition("Hersteller", ColumnType.Text),
            new ColumnTypeDefinition("Hersteller-TeileNr.", ColumnType.Text),
            new ColumnTypeDefinition("Beschreibung", ColumnType.Text),
            new ColumnTypeDefinition("Verfügbarkeit", ColumnType.Text),
            new ColumnTypeDefinition("Mengenpreise", ColumnType.Text) };
        public static int AMOUNT_OF_ATTRIBUTES = columnDefinitions.Length;


        protected override void OnStartup(StartupEventArgs e)
        {


            ObservableCollection<string> attributes = ColumnDefinitionsToOC(columnDefinitions);
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel(attributes);

            mainWindow = new MainWindow();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(optionsWindowViewModel, mainWindow.MainStatusBlock);
            mainWindow.DataContext = mainWindowViewModel;

            mainWindow.Show();
            base.OnStartup(e);
        }


        private ObservableCollection<string> ColumnDefinitionsToOC(ColumnTypeDefinition[] arr)
        {
            ObservableCollection<string> oc = new ObservableCollection<string>();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].type != ColumnType.DontDisplay)
                    oc.Add(arr[i].text);
            }
            return oc;
        }
    }
}
