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
    /// its family, size (in pixels, as FontSize is) and face, its weight, stretch and style. OK sets all of them, the family last,
    /// each through its own row, so that each change raises PropertyValueChanged; with several elements selected, each gets them.
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


        /// <summary>Opens the font dialog on the element's font, and gives it the font chosen.</summary>
        public override void ShowDialog(PropertyItem item, Window owner)
        {
            var initial = FontOf(item.Component);
            var chosen = DesignerFontDialog.Pick(owner, initial);
            if (chosen != null)
                Apply(item, chosen);
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
