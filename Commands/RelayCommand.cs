using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AI_Knowledge_Generator.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _executeAsync;

        public RelayCommand(Action executeAction, Func<bool>? canExecute = null)
        {
            _execute = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecute = canExecute;
        }

        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public async void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute();
            }
            else if (_executeAsync != null)
            {
                await _executeAsync();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}