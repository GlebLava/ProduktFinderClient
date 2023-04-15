using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Commands
{
    public class FastCommandAsync : AsyncCommandBase
    {
        Func<object, Task> Command;
        public FastCommandAsync(Func<object, Task> Command)
        {
            this.Command = Command;
        } 


        protected override async Task ExecuteAsync(object parameter)
        {
            await Command(parameter);
        }
    }
}
