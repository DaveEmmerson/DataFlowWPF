using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataFlowWPF
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase() { }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
