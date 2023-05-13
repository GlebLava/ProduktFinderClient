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
        public static MainWindow mainWindow = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel();

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(optionsWindowViewModel, mainWindow.MainPartsGrid, mainWindow.MainStatusBlock);
            mainWindow.DataContext = mainWindowViewModel;


            mainWindow.Show();
            base.OnStartup(e);
        }

    }
}
