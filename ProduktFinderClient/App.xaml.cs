using Microsoft.AspNetCore.SignalR.Client;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using ProduktFinderClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProduktFinderClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>

public partial class App : Application
{
    public static event EventHandler? MainWindowCloseEvent;

    public static MainWindow mainWindow = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        OptionsWindowViewModel optionsWindowViewModel = new OptionsWindowViewModel();

        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(optionsWindowViewModel, mainWindow.MainPartsGrid, mainWindow.MainStatusBlock);
        mainWindow.DataContext = mainWindowViewModel;

        mainWindow.Show();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainWindowCloseEvent?.Invoke(null, EventArgs.Empty);
        // For some reason when I tried to have Unregister subscribed to MainWindowCloseEvent
        // it would not be waited upon. Because this is a very important call, I explicitly put it here
        // If a somebody knows why, they can put it back into RequestHandler and have it to be called through a
        // subscription
        RequestHandler.Unregister().Wait();
        base.OnExit(e);
    }

}
