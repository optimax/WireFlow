using System.Windows;
using OptimaxTools.FontSelection;

namespace WireFlow.Views
{
    /// <summary>
    /// The font dialog of Optimax's programs (OptimaxTools.FontSelection, MaxPad's and MaxWrite's) in the Designer's colors, as
    /// its other dialogs have them (TextPositionDialog); its controls take the theme's own looks.
    /// </summary>
    public sealed class DesignerFontDialog : FontDialog
    {
        public DesignerFontDialog(FontChoice initial, double unitInPixels = 1)
            : base(initial, unitInPixels)
        {
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            foreach (var button in Buttons)
                button.MinWidth = 80;
        }


        /// <summary>Shows the dialog over <paramref name="owner"/>, opened on <paramref name="initial"/>: the font chosen, or null for Cancel.</summary>
        public static new FontChoice Pick(Window owner, FontChoice initial, double unitInPixels = 1)
        {
            var dialog = new DesignerFontDialog(initial, unitInPixels) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.Chosen : null;
        }
    }
}
