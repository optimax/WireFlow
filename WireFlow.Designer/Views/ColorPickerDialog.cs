using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ColorPicker;
using MaxServices.Core.Themes;

namespace WireFlow.Views
{
    /// <summary>
    /// The color dialog of Optimax's programs, as MarkdownTools has it (MaxPad's ColorPickerDialog): PixiEditor's square of
    /// shades at the left; at its right the RGB, HSV and HSL sliders with alpha, sharing its color; under them the opacity in
    /// percent and the color as #AARRGGBB. Under all of them, swatches: black, white, light gray and gray, and the last colors
    /// chosen (RecentColors), each of which a click makes the color. Enter belongs to the pickers' number fields, so OK is not
    /// the default button.
    /// </summary>
    public sealed class ColorPickerDialog : Window
    {
        private static readonly DependencyPropertyDescriptor ColorProperty =
            DependencyPropertyDescriptor.FromName("SelectedColor", typeof(SquarePicker), typeof(SquarePicker));

        private const double FieldWidth = 130;
        private const int RecentCount = 10;

        private static readonly Color[] StandardColors =
        {
            Colors.Black, Colors.White, Color.FromRgb(0xDD, 0xDD, 0xDD), Color.FromRgb(0x99, 0x99, 0x99),
        };

        // While the fields are given the color, what they say is not taken for one; and the field whose text is being taken
        // as the color is not written over.
        private bool showing;
        private TextBox taking;

        private readonly SquarePicker picker;
        private readonly TextBox opacityBox;
        private readonly TextBox hex;


        /// <summary>
        /// The colors last chosen, the latest first, as #AARRGGBB: at most ten. The Designer gives the dialog the list its settings
        /// keep, so that it is saved with them; choosing a color with OK puts it first.
        /// </summary>
        public static IList<string> RecentColors { get; set; } = new List<string>();


        public ColorPickerDialog(string title, Color initial)
        {
            Title = title;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            SourceInitialized += (s, e) => AppTheme.ApplyTitleBar(this);

            picker = new SquarePicker { SelectedColor = initial, Width = 250, Height = 250, VerticalAlignment = VerticalAlignment.Top };
            var sliders = new ColorSliders { ShowAlpha = true, Width = 300 };
            // Its number fields are 40 wide: the theme's text box, without its least width and wide padding, fits them
            var compact = new Style(typeof(TextBox), TryFindResource(typeof(TextBox)) as Style);
            compact.Setters.Add(new Setter(MinWidthProperty, 0.0));
            compact.Setters.Add(new Setter(MinHeightProperty, 0.0));
            compact.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(2, 1, 2, 1)));
            compact.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            sliders.Resources[typeof(TextBox)] = compact;
            BindingOperations.SetBinding(sliders, PickerControlBase.ColorStateProperty,
                new Binding(nameof(PickerControlBase.ColorState)) { Source = picker, Mode = BindingMode.TwoWay });

            opacityBox = new TextBox { Width = FieldWidth, VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "Opacity: 0 to 100%" };
            hex = new TextBox
            {
                Width = FieldWidth,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Web color: #AARRGGBB, the opacity, red, green and blue in two hexadecimal digits each; #RRGGBB is opaque"
            };
            ShowFields();
            EventHandler colorChanged = (s, e) => ShowFields();
            ColorProperty.AddValueChanged(picker, colorChanged);
            Closed += (s, e) => ColorProperty.RemoveValueChanged(picker, colorChanged);
            opacityBox.TextChanged += (s, e) => TakeOpacity();
            opacityBox.LostKeyboardFocus += (s, e) => ShowFields();
            hex.TextChanged += (s, e) => TakeHex();
            hex.LostKeyboardFocus += (s, e) => ShowFields();

            var fields = new Grid { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 16, 10, 0) };
            fields.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            fields.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            AddField(fields, "Opacity %:", opacityBox);
            AddField(fields, "Web color:", hex);

            var swatches = new StackPanel { Margin = new Thickness(0, 16, 0, 0) };
            swatches.Children.Add(SwatchRow("Standard:", StandardColors));
            var recent = RecentColors?.Select(ParseColor).Where(c => c.HasValue).Select(c => c.Value).Take(RecentCount).ToList() ?? new List<Color>();
            if (recent.Count > 0)
                swatches.Children.Add(SwatchRow("Recent:", recent));

            var ok = new Button { Content = "OK", MinWidth = 80 };
            ok.Click += (s, e) => DialogResult = true;
            var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80, Margin = new Thickness(8, 0, 0, 0) };
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0),
                Children = { ok, cancel }
            };

            var controls = new StackPanel { Margin = new Thickness(16, 0, 0, 0), Children = { sliders, fields } };
            var grid = new Grid { Margin = new Thickness(16) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(controls, 1);
            Grid.SetRow(swatches, 1);
            Grid.SetColumnSpan(swatches, 2);
            Grid.SetRow(buttons, 2);
            Grid.SetColumnSpan(buttons, 2);
            grid.Children.Add(picker);
            grid.Children.Add(controls);
            grid.Children.Add(swatches);
            grid.Children.Add(buttons);
            Content = grid;
        }


        public Color SelectedColor => picker.SelectedColor;


        /// <summary>Shows the dialog over its owner; the color chosen, or null if it was cancelled. A color chosen is put first among the recent ones.</summary>
        public static Color? Show(Window owner, string title, Color initial)
        {
            var dialog = new ColorPickerDialog(title, initial) { Owner = owner };
            if (dialog.ShowDialog() != true)
                return null;
            Remember(dialog.SelectedColor);
            return dialog.SelectedColor;
        }


        /// <summary>Puts a color first among the recent ones, once, and keeps the latest ten.</summary>
        public static void Remember(Color color)
        {
            if (RecentColors == null)
                return;
            string text = ToText(color);
            for (int i = RecentColors.Count - 1; i >= 0; i--)
                if (string.Equals(RecentColors[i], text, StringComparison.OrdinalIgnoreCase))
                    RecentColors.RemoveAt(i);
            RecentColors.Insert(0, text);
            while (RecentColors.Count > RecentCount)
                RecentColors.RemoveAt(RecentColors.Count - 1);
        }


        private static string ToText(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";


        private static Color? ParseColor(string text)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text) ? null : (Color?)ColorConverter.ConvertFromString(text);
            }
            catch (FormatException)
            {
                return null;
            }
        }


        // A row of swatches with its label; a click on one makes its color the dialog's
        private FrameworkElement SwatchRow(string label, IEnumerable<Color> colors)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            row.Children.Add(new TextBlock { Text = label, Width = 70, VerticalAlignment = VerticalAlignment.Center });
            foreach (var color in colors)
            {
                var swatch = new Button
                {
                    Width = 24,
                    Height = 24,
                    MinWidth = 0,
                    MinHeight = 0,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 6, 0),
                    Focusable = false,
                    ToolTip = ToText(color),
                    Template = SwatchTemplate,
                    Background = new SolidColorBrush(color),
                };
                swatch.Click += (s, e) => picker.SelectedColor = color;
                row.Children.Add(swatch);
            }
            return row;
        }


        // A swatch: its color in a square with a gray frame, over a checkerboard where the color is translucent
        private static readonly ControlTemplate SwatchTemplate = MakeSwatchTemplate();

        private static ControlTemplate MakeSwatchTemplate()
        {
            var checker = new DrawingBrush
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 8, 8),
                ViewportUnits = BrushMappingMode.Absolute,
                Drawing = new GeometryDrawing(Brushes.LightGray, null, Geometry.Parse("M0,0 H1 V1 H0Z M1,1 H2 V2 H1Z")),
            };
            checker.Freeze();
            var outer = new FrameworkElementFactory(typeof(Border));
            outer.SetValue(Border.BorderBrushProperty, Brushes.Gray);
            outer.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            outer.SetValue(Border.BackgroundProperty, checker);
            var inner = new FrameworkElementFactory(typeof(Border));
            inner.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
            outer.AppendChild(inner);
            return new ControlTemplate(typeof(Button)) { VisualTree = outer };
        }


        // A row of the fields under the sliders: its label, at the right of the first column, and its box.
        private static void AddField(Grid fields, string label, FrameworkElement box)
        {
            int row = fields.RowDefinitions.Count;
            fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            double above = row == 0 ? 0 : 12;
            var text = new TextBlock
            {
                Text = label,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, above, 8, 0)
            };
            box.Margin = new Thickness(0, above, 0, 0);
            Grid.SetRow(text, row);
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            fields.Children.Add(text);
            fields.Children.Add(box);
        }


        // The fields say the color: its alpha as a percentage, and the color as #AARRGGBB (but the one being typed in).
        private void ShowFields()
        {
            if (showing)
                return;
            Color color = picker.SelectedColor;
            showing = true;
            try
            {
                if (taking != opacityBox)
                    opacityBox.Text = (color.A * 100.0 / 255).ToString("0.#", CultureInfo.CurrentCulture);
                if (taking != hex)
                    hex.Text = ToText(color);
            }
            finally
            {
                showing = false;
            }
        }


        // A percentage typed in the field is the color's alpha (0 to 100; anything else is left until the field is left).
        private void TakeOpacity()
        {
            if (showing || !double.TryParse(opacityBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double percent) || !double.IsFinite(percent))
                return;
            Color color = picker.SelectedColor;
            color.A = (byte)Math.Round(Math.Clamp(percent, 0, 100) * 255 / 100);
            Take(opacityBox, color);
        }


        // #AARRGGBB typed in the field is the color, and #RRGGBB an opaque one; the # may be left out.
        private void TakeHex()
        {
            string digits = hex.Text.Trim().TrimStart('#');
            if (showing || (digits.Length != 6 && digits.Length != 8) ||
                !uint.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint value))
                return;
            if (digits.Length == 6)
                value |= 0xFF000000;
            Take(hex, Color.FromArgb((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value));
        }


        private void Take(TextBox field, Color color)
        {
            taking = field;
            try
            {
                picker.SelectedColor = color;
            }
            finally
            {
                taking = null;
            }
        }
    }
}
