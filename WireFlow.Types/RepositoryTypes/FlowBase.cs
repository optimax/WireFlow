using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace WireFlow
{
    /// <summary>
    /// A node of the DOM: the project, a page or layout, a layer, an element. It tells of each change of its properties
    /// (PropertyChanged), and knows the node it belongs to (Parent), which the collection holding it sets.
    /// </summary>
    public class FlowBase : INotifyPropertyChanged
    {
        private string name;
        private string type;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        public string Type
        {
            get => type;
            set => Set(ref type, value);
        }

        /// <summary>The node holding this one: a layer's page, an element's layer or group; null for a root.</summary>
        [Browsable(false), IgnoreDataMember]
        public FlowBase Parent { get; internal set; }


        /// <summary>Sets a property's field and tells of the change, if the value changed.</summary>
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
