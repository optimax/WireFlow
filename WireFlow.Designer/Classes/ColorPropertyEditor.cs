using System.Windows;
using System.Windows.Media;
using OptimaxTools.Wpf;
using WireFlow.Views;

namespace WireFlow
{
    /// <summary>
    /// The Designer's editor of brushes and colors: the grid's swatch and text, and a "..." button that opens the color
    /// dialog. A brush property gets a solid brush of the color chosen; a gradient or image brush starts the dialog from white.
    /// </summary>
    public class ColorPropertyEditor : BrushPropertyEditor
    {
        /// <summary>Makes it the editor of every Brush and Color property in the property grid.</summary>
        public static void Register()
        {
            var editor = new ColorPropertyEditor();
            PropertyEditors.Register(typeof(Brush), editor);
            PropertyEditors.Register(typeof(Color), editor);
            PropertyEditors.Register(typeof(Color?), editor);
        }


        public override bool HasDialog(PropertyItem item) => true;


        public override void ShowDialog(PropertyItem item, Window owner)
        {
            Color initial = item.Value switch
            {
                Color color => color,
                SolidColorBrush brush => brush.Color,
                _ => Colors.White
            };
            Color? chosen = ColorPickerDialog.Show(owner, item.DisplayName, initial);
            if (chosen == null)
                return;
            if (typeof(Brush).IsAssignableFrom(item.PropertyType))
                item.SetValue(new SolidColorBrush(chosen.Value));
            else
                item.SetValue(chosen.Value);
        }
    }
}
