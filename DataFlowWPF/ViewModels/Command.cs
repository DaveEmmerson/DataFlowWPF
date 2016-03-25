using System;
using System.Windows.Input;
using System.Windows;

namespace DataFlowWPF
{
    public class Command : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public Command(Func<object, bool> canExecute, Action<object> execute)
        {
            Guard.NotNull(canExecute);
            Guard.NotNull(execute);

            _canExecute = canExecute;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            Application.Current.Dispatcher.Invoke(() => CanExecuteChanged?.Invoke(this, null));
        }
    }
}