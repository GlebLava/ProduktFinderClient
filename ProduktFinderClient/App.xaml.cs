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

        protected override void OnStartup(StartupEventArgs e)
        {
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel();

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
