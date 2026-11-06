using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OptimaxTools.Wpf
{
    /// <summary>
    /// One property of a PropertyGrid's selected object: its descriptor, its value, and the editor that edits it. Text goes
    /// through the property's TypeConverter in the invariant culture, as in XAML: "Auto" for an automatic size, "1,2,3,4"
    /// for a Thickness, "#FFFF0000" for a brush. A value that cannot be set leaves the property as it was and sets Error.
    /// </summary>
    public class PropertyItem : INotifyPropertyChanged
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private readonly PropertyGrid grid;
        private string error;
        private bool watching;


        internal PropertyItem(PropertyGrid grid, object component, PropertyDescriptor descriptor)
        {
            this.grid = grid;
            Component = component;
            Descriptor = descriptor;
            Editor = PropertyEditors.Find(this);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>The object whose property this is.</summary>
        public object Component { get; }

        public PropertyDescriptor Descriptor { get; }

        /// <summary>The editor chosen for the property when it was listed (see PropertyEditors).</summary>
        public PropertyEditor Editor { get; }

        public string Name => Descriptor.Name;

        public string DisplayName => Descriptor.DisplayName;

        public string Category => Descriptor.Category;

        public string Description => Descriptor.Description;

        public Type PropertyType => Descriptor.PropertyType;

        public bool IsReadOnly => Descriptor.IsReadOnly;

        public TypeConverter Converter => Descriptor.Converter;

        /// <summary>Whether a value can be typed: the property is writable and its converter reads text.</summary>
        public bool CanEditText => !IsReadOnly && Converter != null && Converter.CanConvertFrom(typeof(string));

        /// <summary>The name's tooltip: the property's description, or its name when it has none.</summary>
        public string NameToolTip => string.IsNullOrEmpty(Description) ? DisplayName : DisplayName + "\n" + Description;

        /// <summary>Why the last value could not be set, or null.</summary>
        public string Error
        {
            get => error;
            private set
            {
                if (error == value)
                    return;
                error = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => error != null;

        /// <summary>The property's value; setting it sets the property, as SetValue does.</summary>
        public object Value
        {
            get
            {
                try
                {
                    return Descriptor.GetValue(Component);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set => SetValue(value);
        }

        /// <summary>The property's value as text; setting it sets the property, as SetText does.</summary>
        public string Text
        {
            get => ToText(Value);
            set => SetText(value);
        }


        /// <summary>
        /// A value as text, through the property's converter, or its ToString when the converter cannot write it.
        /// </summary>
        public string ToText(object value)
        {
            if (value == null)
                return string.Empty;
            try
            {
                if (Converter != null && Converter.CanConvertTo(typeof(string)))
                    return Converter.ConvertToString(null, Culture, value);
            }
            catch (Exception)
            {
                // Some converters write only some values, such as BrushConverter, which writes solid colors only
            }
            return value.ToString();
        }


        /// <summary>
        /// Sets the property from text through its converter. Empty text is null for a property of a reference type other
        /// than string, such as a brush. Returns whether the property was set; if not, Error says why.
        /// </summary>
        public bool SetText(string text)
        {
            if (!CanEditText)
                return false;
            object value;
            try
            {
                if (string.IsNullOrEmpty(text) && !PropertyType.IsValueType && PropertyType != typeof(string))
                    value = null;
                else
                    value = Converter.ConvertFromString(null, Culture, text ?? string.Empty);
            }
            catch (Exception ex)
            {
                Error = $"\"{text}\" is not a valid {PropertyType.Name}: {Message(ex)}";
                Refresh();
                return false;
            }
            return SetValue(value);
        }


        /// <summary>
        /// Sets the property, and raises the grid's PropertyValueChanged when its value changed. Null is ignored for a
        /// property of a value type, which cannot hold it: a list editor reports null when its value is not in its list.
        /// Returns whether the property was set; if not, Error says why.
        /// </summary>
        public bool SetValue(object value)
        {
            if (IsReadOnly)
                return false;
            if (value == null && PropertyType.IsValueType && Nullable.GetUnderlyingType(PropertyType) == null)
                return false;

            object oldValue = Value;
            try
            {
                Descriptor.SetValue(Component, value);
            }
            catch (Exception ex)
            {
                Error = Message(ex);
                Refresh();
                return false;
            }
            Error = null;
            Refresh();

            object newValue = Value;
            if (!Equals(oldValue, newValue) && ToText(oldValue) != ToText(newValue))
                grid.OnPropertyValueChanged(this, oldValue, newValue);
            return true;
        }


        /// <summary>Tells the editor to show the property's current value.</summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Text));
        }


        // While the item is listed, a change of the property made elsewhere, such as moving the element, shows in the grid.
        // A dependency property's descriptor holds the handler until it is removed.
        internal void StartWatching()
        {
            if (watching)
                return;
            try
            {
                Descriptor.AddValueChanged(Component, ValueChangedHandler);
                watching = true;
            }
            catch (Exception)
            {
                // A descriptor that cannot report changes shows the value as it was when it was listed
            }
        }


        internal void StopWatching()
        {
            if (!watching)
                return;
            Descriptor.RemoveValueChanged(Component, ValueChangedHandler);
            watching = false;
        }


        private void ValueChangedHandler(object sender, EventArgs e)
        {
            Refresh();
        }


        private static string Message(Exception ex)
        {
            while (ex is TargetInvocationException && ex.InnerException != null)
                ex = ex.InnerException;
            return ex.Message;
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
