using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProduktFinderClient.Commands
{
    public abstract class AsyncCommandBase : ICommand
    {

        private bool isExecuting;

        public bool IsExecuting
        {
            get { return isExecuting; }
            set { isExecuting = value; CanExecuteChanged?.Invoke(this, new EventArgs()); }
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !isExecuting;
        }

        public async void Execute(object? parameter)
        {
            IsExecuting = true;
            await ExecuteAsync(parameter);  //mit try catch kann man hier exceptions handeln
            IsExecuting = false;
        }

        protected abstract Task ExecuteAsync(object? parameter);
    }
}
