using System;
using System.Collections.Generic;
using System.Text;

namespace ProduktFinderClient.Commands
{
    public class FastCommand : CommandBase
    {
        Action<object> Command;
        public FastCommand(Action<object> Command)
        {
            this.Command = Command;
        }


        public override void Execute(object parameter)
        {
            Command(parameter);
        }
    }
}
