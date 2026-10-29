using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WireFlow
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The [CallerMemberName] attribute is not required, but it will allow you to write: OnPropertyChanged(); instead of OnPropertyChanged("SomeProperty");, so you will avoid string constant in your code.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

    }
}
