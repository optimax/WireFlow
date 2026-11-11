using System;
using System.Globalization;
using System.Windows.Data;

namespace WireFlow
{
    /// <summary>Shows a page or layout, named by its key (<see cref="FlowPage.Key"/>), by its name, as the window's <see cref="Lookup"/> gives it.</summary>
    public sealed class PageNameConverter : IValueConverter
    {
        /// <summary>The name of the page or layout with a key; null when there is none (the key is shown).</summary>
        public static Func<string, string> Lookup { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string key ? Lookup?.Invoke(key) ?? key : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
