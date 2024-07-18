using Sdl.MultiSelectComboBox.EventArgs;
using System.Collections;
using System.Windows.Input;

namespace TAMHubApp.Commands
{
    public class SelectedItemsChangedCommand : ICommand
    {
        private readonly Action<ICollection> _updateSelectedItems;
        private readonly Func<bool> _canExecute;

        public SelectedItemsChangedCommand(Action<ICollection> updateSelectedItems, Func<bool> canExecute)
        {
            _updateSelectedItems = updateSelectedItems;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            if (parameter is SelectedItemsChangedEventArgs args)
            {
                _updateSelectedItems?.Invoke(args.Selected);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
