using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OptimaxTools.FontSelection;
using OptimaxTools.Wpf;
using WireFlow.Views;

namespace WireFlow
{
    /// <summary>
    /// The editor of the FontFamily of a text or a box: its name as text, as before, and a "..." button (or Ctrl+Enter, or a
    /// double click on the value) that opens the font dialog of Optimax's programs (DesignerFontDialog) on the element's font:
    /// its family, size (in pixels, as FontSize is) and face, its weight, stretch and style, and the color of its text
    /// (Foreground). OK sets all of them, the family last, each through its own row, so that each change raises
    /// PropertyValueChanged; with several elements selected, each gets them. The color is set only when another was chosen.
    /// </summary>
    public class FontPropertyEditor : TextPropertyEditor
    {
        /// <summary>Makes it the editor of the FontFamily of texts and boxes in the property grid (their nodes' Font).</summary>
        public static void Register()
        {
            PropertyEditors.Register(typeof(string), typeof(NodeProperties), "FontFamily", new FontPropertyEditor());
        }


        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var box = (TextBox)base.CreateEditor(item);
            if (!item.IsReadOnly)
                box.MouseDoubleClick += (sender, e) =>
                {
                    ShowDialog(item, Window.GetWindow(box));
                    e.Handled = true;
                };
            return box;
        }


        public override bool HasDialog(PropertyItem item) => true;


        /// <summary>Opens the font dialog on the element's font and the color of its text, and gives it the font and the color chosen.</summary>
        public override void ShowDialog(PropertyItem item, Window owner)
        {
            var initial = FontOf(item.Component);
            Color? before = ColorOf(item.Component), color = before;
            var chosen = DesignerFontDialog.Pick(owner, initial, ref color, BackdropOf(item.Component));
            if (chosen == null)
                return;
            // The color first: the family goes last
            ApplyColor(item, before, color);
            Apply(item, chosen);
        }


        /// <summary>Gives the element the color chosen for its text, when one was chosen and it is another than the one it had.</summary>
        public static void ApplyColor(PropertyItem item, Color? before, Color? chosen)
        {
            if (chosen != null && chosen != before)
                item.SetComponentProperty("Foreground", chosen.Value.ToString());
        }


        /// <summary>The color of a text's or a box's text (its Foreground); null when it has none, or the selected elements do not share one.</summary>
        public static Color? ColorOf(object component) =>
            ColorNamed(TypeDescriptor.GetProperties(component)["Foreground"]?.GetValue(component) as string);


        /// <summary>
        /// The color a text or a box lies on, which the dialog draws its sample on: the element's own fill, where one element
        /// is selected and its fill hides what is under it, else the background of its page; null when neither is known.
        /// </summary>
        public static Color? BackdropOf(object component)
        {
            if (!(component is NodeProperties shown))
                return null;
            if (shown.Nodes.Count == 1 && shown.Node is FlowElement element && ColorNamed(element.Fill) is Color fill && fill.A == 255)
                return fill;
            return ColorNamed(shown.PageBackground);
        }


        // The color a text names; null for none, or for text that names no color
        private static Color? ColorNamed(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            try
            {
                return (Color)ColorConverter.ConvertFromString(text);
            }
            catch (System.Exception ex) when (ex is System.FormatException || ex is System.NotSupportedException)
            {
                return null;
            }
        }


        /// <summary>
        /// The font of a text or a box as the dialog takes it. A value the selected elements do not share is empty: the family is
        /// then Segoe UI, as a new element's, the size is left empty, and the face is Regular in its parts not shared.
        /// </summary>
        public static FontChoice FontOf(object component)
        {
            var properties = TypeDescriptor.GetProperties(component);
            object Get(string name) => properties[name]?.GetValue(component);
            string family = Get("FontFamily") as string;
            var weight = Get("FontWeight") is int w && w >= 1 && w <= 999 ? FontWeight.FromOpenTypeWeight(w) : FontWeights.Normal;
            var stretch = Get("FontStretch") is FlowFontStretch s ? s.ToWpf() : FontStretches.Normal;
            var style = Get("FontStyle") is FlowFontStyle f ? f.ToWpf() : FontStyles.Normal;
            return new FontChoice(string.IsNullOrWhiteSpace(family) ? "Segoe UI" : family, Size: Get("FontSize") as double?, HasSize: true)
            {
                Face = new FontFace(style, weight, stretch)
            };
        }


        /// <summary>Gives the element the font chosen: its size, when one is, its face, and its family.</summary>
        public static void Apply(PropertyItem item, FontChoice chosen)
        {
            if (chosen.Size is double size && size > 0)
                item.SetComponentProperty("FontSize", size);
            if (chosen.Face is FontFace face)
            {
                item.SetComponentProperty("FontWeight", face.Weight.ToOpenTypeWeight());
                item.SetComponentProperty("FontStretch", face.Stretch.ToFlow());
                item.SetComponentProperty("FontStyle", face.Style.ToFlow());
            }
            if (!string.IsNullOrWhiteSpace(chosen.Family))
                item.SetValue(chosen.Family);
        }
    }
}
