using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SidebarDiagnostics.Commands
{
    public class ActivateCommand : ICommand
    {
        public void Execute(Object parameter)
        {
            Sidebar _sidebar = App.Current.Sidebar;

            if (_sidebar == null)
            {
                return;
            }
            
            _sidebar.Activate();
        }

        public Boolean CanExecute(Object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
