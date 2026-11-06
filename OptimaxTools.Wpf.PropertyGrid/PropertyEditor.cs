using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OptimaxTools.Wpf
{
    /// <summary>
    /// Edits a property in its row of a PropertyGrid. PropertyEditors chooses the editor of each property; register an editor
    /// there for a type of property, or for one property of one type of object, as with Delphi's RegisterPropertyEditor.
    /// The row's implicit styles make TextBox, CheckBox and ComboBox editors compact and give them the grid's colors.
    /// </summary>
    public abstract class PropertyEditor
    {
        /// <summary>The control that shows and edits the property in its row; one is created for each row shown.</summary>
        public abstract FrameworkElement CreateEditor(PropertyItem item);
    }



    /// <summary>
    /// Chooses the editor of each property. A registration names a type of property, which also matches the types derived
    /// from it, and can be narrowed to the objects of a type and to a property name. The most specific registration that
    /// matches wins (property name, then object type, then the exact property type), and the latest of equally specific
    /// ones. Without a matching registration, the editor is chosen by the property's type (see Default).
    /// </summary>
    public static class PropertyEditors
    {
        public static readonly PropertyEditor Text = new TextPropertyEditor();
        public static readonly PropertyEditor Boolean = new BooleanPropertyEditor();
        public static readonly PropertyEditor Choice = new ChoicePropertyEditor();
        public static readonly PropertyEditor FontFamily = new FontFamilyPropertyEditor();
        public static readonly PropertyEditor Brush = new BrushPropertyEditor();

        private static readonly List<Registration> registrations = new List<Registration>();


        /// <summary>Registers an editor for the properties of a type, or of a type derived from it.</summary>
        public static void Register(Type propertyType, PropertyEditor editor)
        {
            Register(propertyType, null, null, editor);
        }


        /// <summary>
        /// Registers an editor for the properties of a type, or of a type derived from it, of the objects of componentType
        /// (or of any object if null) named propertyName (or of any name if null).
        /// </summary>
        public static void Register(Type propertyType, Type componentType, string propertyName, PropertyEditor editor)
        {
            if (propertyType == null)
                throw new ArgumentNullException(nameof(propertyType));
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));
            lock (registrations)
                registrations.Add(new Registration(propertyType, componentType, propertyName, editor));
        }


        /// <summary>The editor of a property: the best registration that matches it, otherwise Default.</summary>
        public static PropertyEditor Find(PropertyItem item)
        {
            Registration best = null;
            int bestScore = -1;
            lock (registrations)
                foreach (var registration in registrations)
                {
                    int score = registration.Score(item);
                    if (score >= bestScore && score >= 0)
                    {
                        best = registration;
                        bestScore = score;
                    }
                }
            return best?.Editor ?? Default(item);
        }


        /// <summary>
        /// The editor of a property without a registration: text for a read-only property; a check box for a Boolean; a
        /// color swatch with text for a brush or a color; a list of fonts for a font family; a list of values for an enum,
        /// a font weight, style or stretch, or a type whose converter offers a closed list of values; text otherwise.
        /// </summary>
        public static PropertyEditor Default(PropertyItem item)
        {
            Type type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;
            if (item.IsReadOnly)
                return Text;
            if (type == typeof(bool))
                return Boolean;
            if (typeof(System.Windows.Media.Brush).IsAssignableFrom(type) || type == typeof(Color))
                return Brush;
            if (type == typeof(System.Windows.Media.FontFamily))
                return FontFamily;
            if (type.IsEnum || ChoicePropertyEditor.KnownValues(type) != null)
                return Choice;
            var converter = item.Converter;
            if (converter != null && converter.GetStandardValuesSupported() && converter.GetStandardValuesExclusive())
                return Choice;
            return Text;
        }


        private sealed class Registration
        {
            private readonly Type propertyType;
            private readonly Type componentType;
            private readonly string propertyName;

            public Registration(Type propertyType, Type componentType, string propertyName, PropertyEditor editor)
            {
                this.propertyType = propertyType;
                this.componentType = componentType;
                this.propertyName = propertyName;
                Editor = editor;
            }

            public PropertyEditor Editor { get; }

            // -1 when the registration does not match the property; otherwise higher for a more specific one
            public int Score(PropertyItem item)
            {
                if (!propertyType.IsAssignableFrom(item.PropertyType))
                    return -1;
                if (componentType != null && !componentType.IsInstanceOfType(item.Component))
                    return -1;
                if (propertyName != null && propertyName != item.Name)
                    return -1;
                return (propertyName != null ? 4 : 0) + (componentType != null ? 2 : 0) + (propertyType == item.PropertyType ? 1 : 0);
            }
        }
    }



    /// <summary>
    /// A text box for the property's value as text (see PropertyItem.Text). Enter or leaving the box sets the property;
    /// Escape gives back the property's value. A property that cannot take text shows its value read-only.
    /// </summary>
    public class TextPropertyEditor : PropertyEditor
    {
        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var box = new TextBox { IsReadOnly = !item.CanEditText };
            box.SetBinding(TextBox.TextProperty, new Binding(nameof(PropertyItem.Text))
            {
                Source = item,
                Mode = box.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
            if (!box.IsReadOnly)
            {
                box.PreviewKeyDown += TextBoxPreviewKeyDownHandler;
                box.LostKeyboardFocus += (sender, e) => Commit((TextBox)sender);
            }
            return box;
        }


        private static void TextBoxPreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            var box = (TextBox)sender;
            if (e.Key == Key.Enter)
            {
                Commit(box);
                box.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                box.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                box.SelectAll();
                e.Handled = true;
            }
        }


        // Sets the property from the text typed, then shows the property's value: the value as its converter writes it, or
        // the old value if the text was not valid. The binding ignores the item's own change notice while it sets the value.
        private static void Commit(TextBox box)
        {
            var binding = box.GetBindingExpression(TextBox.TextProperty);
            if (binding == null)
                return;
            if (binding.IsDirty)
                binding.UpdateSource();
            binding.UpdateTarget();
        }
    }



    /// <summary>A check box for a Boolean property; three states for a nullable one.</summary>
    public class BooleanPropertyEditor : PropertyEditor
    {
        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var check = new CheckBox
            {
                IsThreeState = Nullable.GetUnderlyingType(item.PropertyType) != null,
                IsEnabled = !item.IsReadOnly
            };
            check.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(PropertyItem.Value)) { Source = item, Mode = BindingMode.TwoWay });
            return check;
        }
    }



    /// <summary>
    /// A list of the property's values: an enum's, the named values of a font weight, style or stretch, or the values its
    /// converter offers. A current value that is not in the list is added to it.
    /// </summary>
    public class ChoicePropertyEditor : PropertyEditor
    {
        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var values = GetValues(item).Cast<object>().ToList();
            object current = item.Value;
            if (current != null && !values.Contains(current))
                values.Insert(0, current);
            var combo = new ComboBox { ItemsSource = values, IsEnabled = !item.IsReadOnly };
            combo.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(PropertyItem.Value)) { Source = item, Mode = BindingMode.TwoWay });
            return combo;
        }


        protected virtual IEnumerable GetValues(PropertyItem item)
        {
            Type type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;
            if (type.IsEnum)
                return Enum.GetValues(type);
            var known = KnownValues(type);
            if (known != null)
                return known;
            var converter = item.Converter;
            if (converter != null && converter.GetStandardValuesSupported())
                return (IEnumerable)converter.GetStandardValues() ?? Array.Empty<object>();
            return Array.Empty<object>();
        }


        /// <summary>
        /// The named values of WPF's font types, which their converters do not list: FontWeights, FontStyles and
        /// FontStretches, each value once, lightest or narrowest first. Null for other types.
        /// </summary>
        internal static IList<object> KnownValues(Type type)
        {
            if (type == typeof(FontWeight))
                return NamedValues(typeof(FontWeights), type).OrderBy(v => ((FontWeight)v).ToOpenTypeWeight()).ToList();
            if (type == typeof(FontStretch))
                return NamedValues(typeof(FontStretches), type).OrderBy(v => ((FontStretch)v).ToOpenTypeStretch()).ToList();
            if (type == typeof(FontStyle))
                return NamedValues(typeof(FontStyles), type).ToList();
            return null;
        }


        private static IEnumerable<object> NamedValues(Type holder, Type type) =>
            holder.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == type)
                .Select(p => p.GetValue(null))
                .Distinct();
    }



    /// <summary>A list of the system's font families, by name.</summary>
    public class FontFamilyPropertyEditor : ChoicePropertyEditor
    {
        protected override IEnumerable GetValues(PropertyItem item) =>
            Fonts.SystemFontFamilies.OrderBy(f => f.Source, StringComparer.CurrentCultureIgnoreCase);
    }



    /// <summary>A swatch of a brush's or a color's value beside its text (see TextPropertyEditor).</summary>
    public class BrushPropertyEditor : TextPropertyEditor
    {
        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var text = base.CreateEditor(item);
            var swatch = new Border
            {
                Width = 14,
                Height = 14,
                Margin = new Thickness(3, 0, 1, 0),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = true
            };
            swatch.SetBinding(Border.BackgroundProperty, new Binding(nameof(PropertyItem.Value)) { Source = item, Converter = SwatchConverter.Instance });
            swatch.SetBinding(Border.BorderBrushProperty, new Binding(nameof(Control.Foreground))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(PropertyGrid), 1)
            });
            var panel = new DockPanel();
            DockPanel.SetDock(swatch, System.Windows.Controls.Dock.Left);
            panel.Children.Add(swatch);
            panel.Children.Add(text);
            return panel;
        }


        // A brush as it is; a color as a brush of that color
        private sealed class SwatchConverter : IValueConverter
        {
            public static readonly SwatchConverter Instance = new SwatchConverter();

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
                value is Color color ? new SolidColorBrush(color) : value as System.Windows.Media.Brush;

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
                throw new NotSupportedException();
        }
    }



    /// <summary>Shows the editor of the item of its row, created when the row is.</summary>
    public class PropertyEditorHost : Decorator
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(PropertyItem),
            typeof(PropertyEditorHost), new PropertyMetadata(null, (d, e) => ((PropertyEditorHost)d).ItemChanged()));

        public PropertyItem Item
        {
            get => (PropertyItem)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private void ItemChanged()
        {
            var item = Item;
            Child = item?.Editor.CreateEditor(item);
        }
    }



    /// <summary>The grip between the name and value columns of a PropertyGrid, which sets NameColumnWidth.</summary>
    public class PropertyColumnSplitter : Thumb
    {
        static PropertyColumnSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyColumnSplitter), new FrameworkPropertyMetadata(typeof(PropertyColumnSplitter)));
        }
    }
}
