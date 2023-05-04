using Microsoft.Win32;
using ProduktFinderClient.Components;
using ProduktFinderClient.CSV;
using ProduktFinderClient.Models.ErrorLogging;
using ProduktFinderClient.ViewModels;
using ProduktFinderClient.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace ProduktFinderClient.Commands
{
    class OpenCSVPreviewCommand : CommandBase
    {

        MainWindowViewModel mainWindowViewModel;
        Func<StatusHandle> UserUpdateStatusHandleCreate;
        public OpenCSVPreviewCommand(MainWindowViewModel mainWindowViewModel, Func<StatusHandle> UserUpdateStatusHandleCreate)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            this.UserUpdateStatusHandleCreate = UserUpdateStatusHandleCreate;
        }


        public override void Execute(object parameter)
        {
            try
            {
                OpenFileDialog openfileDialog = new OpenFileDialog();
                openfileDialog.Filter = "CSV Files (*.csv)|*.csv";
                if (openfileDialog.ShowDialog() == true)
                {
                    CSVPreviewViewModel context = new CSVPreviewViewModel(Path.GetFileName(openfileDialog.FileName), CSVParser.ParseCSVFile(openfileDialog.FileName), mainWindowViewModel, UserUpdateStatusHandleCreate);
                    CSVPreviewWindow window = new CSVPreviewWindow
                    {
                        DataContext = context
                    };
                    window.Show();
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e, "Es wurde kein Keyword gesucht, der error ist in OpenCSVPreviewCommand aufgetreten");
            }
        }
    }
}
