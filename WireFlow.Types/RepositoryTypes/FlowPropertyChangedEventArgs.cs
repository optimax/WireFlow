using System.ComponentModel;

namespace WireFlow
{
    /// <summary>A change of a DOM node's property, with the values before and after it, so that it can be undone.</summary>
    public class FlowPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public object OldValue { get; }
        public object NewValue { get; }


        public FlowPropertyChangedEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
