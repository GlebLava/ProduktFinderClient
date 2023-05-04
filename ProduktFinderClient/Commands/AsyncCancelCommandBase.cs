using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProduktFinderClient.Commands
{
    public abstract class AsyncCancelCommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly string _normalText;
        private readonly string _cancelText;
        private readonly Action<string> _SetButtonContent;
        private bool isExecuting = false;

        protected CancellationTokenSource? cancellationTokenSource = null;


        public AsyncCancelCommandBase(string normalText, string cancelText, Action<string> SetButtonContent)
        {
            _normalText = normalText;
            _cancelText = cancelText;
            _SetButtonContent = SetButtonContent;

            _SetButtonContent(normalText);
        }

        // Since we want the cancel state of the button to always be pressable true is returned
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            try
            {
                if (!isExecuting)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    _SetButtonContent(_cancelText);
                    isExecuting = true;
                    await ExecuteAsync(parameter, cancellationTokenSource.Token);  //mit try catch kann man hier exceptions handeln
                }

                if (isExecuting && cancellationTokenSource is not null)
                {
                    cancellationTokenSource.Cancel();
                }

            }
            finally
            {
                _SetButtonContent(_normalText);
                isExecuting = false;
            }
        }

        protected abstract Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);
    }
}
