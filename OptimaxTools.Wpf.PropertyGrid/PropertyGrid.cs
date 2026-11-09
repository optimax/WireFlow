using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OptimaxTools.Wpf
{
    /// <summary>
    /// Lists the browsable properties of SelectedObject, by category or by name, and edits them with the editors that
    /// PropertyEditors chooses. The header names the object's type and its Name; the filter box keeps the properties whose
    /// names contain its text. PropertyValueChanged is raised for a value set in the grid, not for one changed elsewhere,
    /// though the grid shows those too.
    /// </summary>
    [TemplatePart(Name = "PART_Filter", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ClearFilter", Type = typeof(ButtonBase))]
    public class PropertyGrid : Control
    {
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(nameof(SelectedObject),
            typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).SelectedObjectChanged()));

        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(nameof(NameColumnWidth),
            typeof(double), typeof(PropertyGrid), new PropertyMetadata(150.0));

        public static readonly DependencyProperty IsCategorizedProperty = DependencyProperty.Register(nameof(IsCategorized),
            typeof(bool), typeof(PropertyGrid), new PropertyMetadata(true, (d, e) => ((PropertyGrid)d).ArrangeView()));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter),
            typeof(string), typeof(PropertyGrid), new PropertyMetadata(string.Empty, (d, e) => ((PropertyGrid)d).Properties?.Refresh()));

        public static readonly DependencyProperty FilterWatermarkProperty = DependencyProperty.Register(nameof(FilterWatermark),
            typeof(string), typeof(PropertyGrid), new PropertyMetadata("Filter"));

        public static readonly DependencyProperty CategoryBackgroundProperty = DependencyProperty.Register(nameof(CategoryBackground),
            typeof(Brush), typeof(PropertyGrid), new PropertyMetadata(null));

        private static readonly DependencyPropertyKey PropertiesPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Properties),
            typeof(ICollectionView), typeof(PropertyGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertiesProperty = PropertiesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey SelectedObjectTypeNamePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedObjectTypeName), typeof(string), typeof(PropertyGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedObjectTypeNameProperty = SelectedObjectTypeNamePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey SelectedObjectNamePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedObjectName), typeof(string), typeof(PropertyGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedObjectNameProperty = SelectedObjectNamePropertyKey.DependencyProperty;

        private const double MinColumnWidth = 40;

        private List<PropertyItem> items = new List<PropertyItem>();


        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }


        public PropertyGrid()
        {
            AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(SplitterDragDeltaHandler));
        }


        /// <summary>Raised when a property is set in the grid and its value changes.</summary>
        public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

        /// <summary>The object whose properties are listed.</summary>
        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        /// <summary>The width of the name column; the grip between the columns sets it.</summary>
        public double NameColumnWidth
        {
            get => (double)GetValue(NameColumnWidthProperty);
            set => SetValue(NameColumnWidthProperty, value);
        }

        /// <summary>Whether the properties are grouped by category; otherwise they are listed by name.</summary>
        public bool IsCategorized
        {
            get => (bool)GetValue(IsCategorizedProperty);
            set => SetValue(IsCategorizedProperty, value);
        }

        /// <summary>Text that the listed names contain, ignoring case; empty lists every property.</summary>
        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        /// <summary>The hint shown in the empty filter box.</summary>
        public string FilterWatermark
        {
            get => (string)GetValue(FilterWatermarkProperty);
            set => SetValue(FilterWatermarkProperty, value);
        }

        /// <summary>The background of the category headers; none when null.</summary>
        public Brush CategoryBackground
        {
            get => (Brush)GetValue(CategoryBackgroundProperty);
            set => SetValue(CategoryBackgroundProperty, value);
        }

        /// <summary>The listed properties of SelectedObject, as PropertyItem objects.</summary>
        public ICollectionView Properties => (ICollectionView)GetValue(PropertiesProperty);

        /// <summary>The name of the type of SelectedObject.</summary>
        public string SelectedObjectTypeName => (string)GetValue(SelectedObjectTypeNameProperty);

        /// <summary>The Name property of SelectedObject, if it has one.</summary>
        public string SelectedObjectName => (string)GetValue(SelectedObjectNameProperty);


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_ClearFilter") is ButtonBase clear)
                clear.Click += (sender, e) => Filter = string.Empty;
            if (GetTemplateChild("PART_Filter") is TextBox filter)
                filter.PreviewKeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Escape && !string.IsNullOrEmpty(Filter))
                    {
                        Filter = string.Empty;
                        e.Handled = true;
                    }
                };
        }


        internal void OnPropertyValueChanged(PropertyItem item, object oldValue, object newValue)
        {
            PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(item, oldValue, newValue));
        }


        // Lists the properties of the new object, watching their values while they are listed
        private void SelectedObjectChanged()
        {
            foreach (var item in items)
                item.StopWatching();

            object selected = SelectedObject;
            var listed = new List<PropertyItem>();
            if (selected != null)
            {
                foreach (PropertyDescriptor descriptor in GetProperties(selected))
                    if (descriptor.IsBrowsable)
                        listed.Add(new PropertyItem(this, selected, descriptor));
                foreach (var item in listed)
                    item.StartWatching();
            }
            items = listed;

            // An object that describes itself names its own kind, as its properties are its own
            SetValue(SelectedObjectTypeNamePropertyKey, selected is ICustomTypeDescriptor described ? described.GetClassName() : selected?.GetType().Name);
            SetValue(SelectedObjectNamePropertyKey, NameOf(selected));
            SetValue(PropertiesPropertyKey, new ListCollectionView(listed) { Filter = IsListed });
            ArrangeView();
        }


        // The properties of the object's type, or the ones it describes itself. A type's descriptors serve all its objects;
        // for a WPF element they are the descriptors of its dependency properties, which report changes.
        private static PropertyDescriptorCollection GetProperties(object selected) =>
            selected is ICustomTypeDescriptor custom ? custom.GetProperties() : TypeDescriptor.GetProperties(selected.GetType());


        private static string NameOf(object selected)
        {
            if (selected == null)
                return null;
            try
            {
                return TypeDescriptor.GetProperties(selected)["Name"]?.GetValue(selected) as string;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // Groups the view by category, in the order of the categories' names, or lists it by name only
        private void ArrangeView()
        {
            if (!(Properties is ListCollectionView view))
                return;
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                if (IsCategorized)
                    view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PropertyItem.Category)));
                view.CustomSort = new ItemComparer(IsCategorized);
            }
        }


        private bool IsListed(object item)
        {
            string filter = Filter;
            return string.IsNullOrEmpty(filter) || ((PropertyItem)item).DisplayName.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }


        private void SplitterDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            if (!(e.OriginalSource is PropertyColumnSplitter))
                return;
            double max = Math.Max(MinColumnWidth, ActualWidth - MinColumnWidth);
            NameColumnWidth = Math.Min(max, Math.Max(MinColumnWidth, NameColumnWidth + e.HorizontalChange));
            e.Handled = true;
        }


        private sealed class ItemComparer : IComparer
        {
            private readonly bool byCategory;

            public ItemComparer(bool byCategory)
            {
                this.byCategory = byCategory;
            }

            public int Compare(object x, object y)
            {
                var a = (PropertyItem)x;
                var b = (PropertyItem)y;
                var comparer = StringComparer.CurrentCultureIgnoreCase;
                int result = byCategory ? comparer.Compare(a.Category, b.Category) : 0;
                return result != 0 ? result : comparer.Compare(a.DisplayName, b.DisplayName);
            }
        }
    }



    public class PropertyValueChangedEventArgs : EventArgs
    {
        public PropertyValueChangedEventArgs(PropertyItem item, object oldValue, object newValue)
        {
            Item = item;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public PropertyItem Item { get; }
        public object OldValue { get; }
        public object NewValue { get; }
    }



    /// <summary>Negates a Boolean, both ways: the Alphabetical button is checked when IsCategorized is false.</summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public static readonly InverseBooleanConverter Instance = new InverseBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value is bool b && b);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(value is bool b && b);
    }
}
