using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using OptimaxTools.FontSelection;

namespace WireFlow.Views
{
    /// <summary>
    /// The font dialog of Optimax's programs (OptimaxTools.FontSelection, MaxPad's and MaxWrite's) in the Designer's colors, as
    /// its other dialogs have them (TextPositionDialog); its controls take the theme's own looks. The Designer's is wider than
    /// it is tall: the sample lies beside the lists, not under them, and over it is the color of the text, a button showing
    /// it as a swatch and as #AARRGGBB, which opens the color dialog (ColorPickerDialog) on it. The sample is drawn in that
    /// color, on the color the text lies on (<see cref="Backdrop"/>).
    /// </summary>
    public sealed class DesignerFontDialog : FontDialog
    {
        // The width of the column beside the lists, which holds the color and the sample
        private const double SideWidth = 270;

        private readonly Border swatch = new Border { Width = 16, Height = 16, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        private readonly TextBlock colorText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        private Color? textColor;
        private Color? backdrop;


        public DesignerFontDialog(FontChoice initial, double unitInPixels = 1)
            : base(initial, unitInPixels)
        {
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            foreach (var button in Buttons)
                button.MinWidth = 80;
            Arrange();
            ShowColor();
        }


        /// <summary>
        /// The color of the text: the one the dialog opens on, and, after OK, the one chosen. Null is none: the elements
        /// selected do not share one, and none was chosen.
        /// </summary>
        public Color? TextColor
        {
            get { return textColor; }
            set
            {
                textColor = value;
                ShowColor();
            }
        }


        /// <summary>The color the text lies on, which the sample is drawn on; null leaves the sample on the dialog's own.</summary>
        public Color? Backdrop
        {
            get { return backdrop; }
            set
            {
                backdrop = value;
                ShowColor();
            }
        }


        /// <summary>The button showing the color of the text, which opens the color dialog.</summary>
        public Button ColorButton { get; private set; }


        // Puts the sample beside the lists, with the color of the text over it, where the dialog is laid out as this was
        // written for (three columns of lists, the sample under them, the buttons last); otherwise it is left as it is
        private void Arrange()
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(swatch);
            content.Children.Add(colorText);
            swatch.SetBinding(Border.BorderBrushProperty, new Binding(nameof(Foreground)) { Source = this });
            ColorButton = new Button
            {
                Content = content,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 4),
                ToolTip = ToolTipOf("The color of the text. Click to choose another."),
            };
            AutomationProperties.SetName(ColorButton, "Color");
            ColorButton.Click += (sender, e) =>
            {
                Color? chosen = ColorPickerDialog.Show(this, "Foreground", textColor ?? Colors.White);
                if (chosen != null)
                    TextColor = chosen;
            };
            // As tall as the boxes over the lists, so that the sample starts where the lists do
            ColorButton.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = FamilyBox });
            var label = new TextBlock { Text = "Color:", Margin = new Thickness(0, 0, 0, 2) };
            var side = new DockPanel();
            DockPanel.SetDock(label, Dock.Top);
            DockPanel.SetDock(ColorButton, Dock.Top);
            side.Children.Add(label);
            side.Children.Add(ColorButton);

            GroupBox sample = null;
            for (DependencyObject parent = Sample; parent != null && sample == null; parent = LogicalTreeHelper.GetParent(parent))
                sample = parent as GroupBox;
            if (!(Content is Grid grid) || sample == null || sample.Parent != grid || !(OkButton.Parent is UIElement buttons) || grid.ColumnDefinitions.Count != 3)
            {
                // A layout not known: the color goes under what there is
                if (Content is Grid other)
                {
                    other.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    Grid.SetRow(side, other.RowDefinitions.Count - 1);
                    Grid.SetColumnSpan(side, System.Math.Max(1, other.ColumnDefinitions.Count));
                    other.Children.Add(side);
                }
                return;
            }
            int at = grid.Children.IndexOf(sample);
            grid.Children.Remove(sample);
            // The sample fills the column under the color, as tall as the lists beside it, whatever height it had
            sample.Margin = new Thickness(0);
            sample.ClearValue(HeightProperty);
            sample.VerticalAlignment = VerticalAlignment.Stretch;
            if (Sample.Parent is FrameworkElement frame)
                frame.ClearValue(HeightProperty);
            side.Children.Add(sample);
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SideWidth) });
            Grid.SetRow(side, 0);
            Grid.SetColumn(side, 3);
            // Where the sample was, so that Tab goes from the lists to the color and the sample, then to the buttons
            grid.Children.Insert(at, side);
            Grid.SetColumnSpan(buttons, 4);
            // The column is narrower than the dialog was: a long sample goes on to the next line
            Sample.TextWrapping = TextWrapping.Wrap;
        }


        // Shows the color of the text on its button, and the sample in it, on the color the text lies on
        private void ShowColor()
        {
            swatch.Background = textColor is Color color ? new SolidColorBrush(color) : null;
            colorText.Text = textColor?.ToString() ?? "";
            if (textColor is Color shown)
                Sample.Foreground = new SolidColorBrush(shown);
            else
                Sample.ClearValue(TextBlock.ForegroundProperty);
            if (Sample.Parent is Border frame)
            {
                if (backdrop is Color behind)
                    frame.Background = new SolidColorBrush(behind);
                else
                    frame.ClearValue(Border.BackgroundProperty);
            }
        }


        /// <summary>Shows the dialog over <paramref name="owner"/>, opened on <paramref name="initial"/>: the font chosen, or null for Cancel.</summary>
        public static new FontChoice Pick(Window owner, FontChoice initial, double unitInPixels = 1)
        {
            var dialog = new DesignerFontDialog(initial, unitInPixels) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.Chosen : null;
        }


        /// <summary>
        /// Shows the dialog over <paramref name="owner"/>, opened on <paramref name="initial"/> and on the text's color, with
        /// its sample on <paramref name="backdrop"/>: the font chosen, with the color chosen in <paramref name="color"/>, or
        /// null for Cancel, which leaves the color as it was.
        /// </summary>
        public static FontChoice Pick(Window owner, FontChoice initial, ref Color? color, Color? backdrop, double unitInPixels = 1)
        {
            var dialog = new DesignerFontDialog(initial, unitInPixels) { Owner = owner, TextColor = color, Backdrop = backdrop };
            if (dialog.ShowDialog() != true)
                return null;
            color = dialog.TextColor;
            return dialog.Chosen;
        }
    }
}
