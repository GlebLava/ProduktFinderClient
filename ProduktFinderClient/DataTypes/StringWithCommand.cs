using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ProduktFinderClient.Commands;

namespace ProduktFinderClient.DataTypes
{
    public class StringWithCommand : PathableDpds
    {
        private ICommand command;
        private string text;

        public ICommand Command
        {
            get { return command; }
            set { command = value; OnPropertyChanged(nameof(Command)); }
        }
        public string Text
        {
            get { return text; }
            set { text = value; OnPropertyChanged(nameof(Text)); }
        }

        public StringWithCommand(Action<object> Action, string Text)
        {
            command = new FastCommand(Action);
            this.Text = Text;
        }
    }
}
