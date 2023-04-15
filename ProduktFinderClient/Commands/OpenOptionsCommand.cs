using ProduktFinderClient.ViewModels;
using ProduktFinderClient.Views;
using System.Windows.Input;

namespace ProduktFinderClient.Commands
{
    public class OpenOptionsCommand : CommandBase
    {
        readonly OptionsWindowViewModel optionsWindowViewModel;
        OptionsWindow existingOptionsWindow;

        public OpenOptionsCommand(OptionsWindowViewModel optionsWindowViewModel)
        {
            this.optionsWindowViewModel = optionsWindowViewModel;
        }

        public override void Execute(object parameter)
        {
            if (existingOptionsWindow == null || !existingOptionsWindow.IsLoaded)
            {

                OptionsWindow optionsWindow = new OptionsWindow();

                optionsWindow.DataContext = optionsWindowViewModel;

                optionsWindow.Top = App.mainWindow.PointToScreen(Mouse.GetPosition(App.mainWindow)).Y;
                optionsWindow.Left = App.mainWindow.PointToScreen(Mouse.GetPosition(App.mainWindow)).X;


                optionsWindow.Show();
                existingOptionsWindow = optionsWindow;

            }
            else
            {
                existingOptionsWindow.Activate();
            }

        }
    }
}
