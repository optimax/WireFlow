using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using OptimaxTools.Wpf;
using WireFlow.Views;

namespace WireFlow
{
    /// <summary>
    /// The Designer's editor of colors: the grid's swatch and text, and a "..." button that opens the color dialog. A brush
    /// property gets a solid brush of the color chosen (a gradient or image brush starts the dialog from white); a color kept
    /// as text in the DOM (#AARRGGBB, or a name such as White) gets the color's text.
    /// </summary>
    public class ColorPropertyEditor : BrushPropertyEditor
    {
        // The DOM's properties that hold a color as text, on the nodes Properties shows
        private static readonly string[] ColorTextProperties = { "Fill", "Stroke", "Foreground", "Background", "BorderColor" };


        /// <summary>Makes it the editor of every Brush and Color property in the property grid, and of the DOM's colors.</summary>
        public static void Register()
        {
            var editor = new ColorPropertyEditor();
            PropertyEditors.Register(typeof(Brush), editor);
            PropertyEditors.Register(typeof(Color), editor);
            PropertyEditors.Register(typeof(Color?), editor);
            foreach (var name in ColorTextProperties)
                PropertyEditors.Register(typeof(string), typeof(NodeProperties), name, editor);
        }


        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var editor = base.CreateEditor(item);
            // The swatch of a color as text shows the color it names
            if (item.PropertyType == typeof(string) && editor is DockPanel panel && panel.Children.Count > 0 && panel.Children[0] is Border swatch)
                swatch.SetBinding(Border.BackgroundProperty, new Binding(nameof(PropertyItem.Value)) { Source = item, Converter = ColorTextConverter.Instance });
            return editor;
        }


        public override bool HasDialog(PropertyItem item) => true;


        public override void ShowDialog(PropertyItem item, Window owner)
        {
            Color initial = item.Value switch
            {
                Color color => color,
                SolidColorBrush brush => brush.Color,
                string text => ColorOf(text) ?? Colors.White,
                _ => Colors.White
            };
            Color? chosen = ColorPickerDialog.Show(owner, item.DisplayName, initial);
            if (chosen == null)
                return;
            if (item.PropertyType == typeof(string))
                item.SetValue(chosen.Value.ToString());
            else if (typeof(Brush).IsAssignableFrom(item.PropertyType))
                item.SetValue(new SolidColorBrush(chosen.Value));
            else
                item.SetValue(chosen.Value);
        }


        // The color a text names; null for none, or for text that names no color
        private static Color? ColorOf(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            try
            {
                return (Color)ColorConverter.ConvertFromString(text);
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
                return null;
            }
        }


        private sealed class ColorTextConverter : IValueConverter
        {
            public static readonly ColorTextConverter Instance = new ColorTextConverter();

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
                ColorOf(value as string) is Color color ? new SolidColorBrush(color) : null;

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
                throw new NotSupportedException();
        }
    }
}
