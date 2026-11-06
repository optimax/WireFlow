using System;
using System.ComponentModel;
using System.Globalization;
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
    /// shades at the left; at its right the RGB, HSV and HSL sliders with alpha, sharing its color; under them the color
    /// and a second one to swap with, the opacity in percent, and the color as #AARRGGBB. Enter belongs to the pickers'
    /// number fields, so OK is not the default button.
    /// </summary>
    public sealed class ColorPickerDialog : Window
    {
        private static readonly DependencyPropertyDescriptor ColorProperty =
            DependencyPropertyDescriptor.FromName("SelectedColor", typeof(SquarePicker), typeof(SquarePicker));

        private const double FieldWidth = 90;

        // While the fields are given the color, what they say is not taken for one; and the field whose text is being taken
        // as the color is not written over.
        private bool showing;
        private TextBox taking;

        private readonly SquarePicker picker;
        private readonly TextBox opacityBox;
        private readonly TextBox hex;


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
            var sliders = new ColorSliders { ShowAlpha = true, Width = 290 };
            var display = new ColorDisplay { Width = 56, Height = 56, VerticalAlignment = VerticalAlignment.Center };
            foreach (PickerControlBase shared in new PickerControlBase[] { sliders, display })
                BindingOperations.SetBinding(shared, PickerControlBase.ColorStateProperty,
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

            var fields = new Grid { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top };
            fields.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            fields.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            AddField(fields, "Opacity %:", opacityBox);
            AddField(fields, "Web color:", hex);

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

            var below = new DockPanel { Margin = new Thickness(10, 2, 10, 0), Children = { display, fields } };
            var controls = new StackPanel { Margin = new Thickness(16, 0, 0, 0), Children = { sliders, below } };
            var grid = new Grid { Margin = new Thickness(16) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(controls, 1);
            Grid.SetRow(buttons, 1);
            Grid.SetColumnSpan(buttons, 2);
            grid.Children.Add(picker);
            grid.Children.Add(controls);
            grid.Children.Add(buttons);
            Content = grid;
        }


        public Color SelectedColor => picker.SelectedColor;


        /// <summary>Shows the dialog over its owner; the color chosen, or null if it was cancelled.</summary>
        public static Color? Show(Window owner, string title, Color initial)
        {
            var dialog = new ColorPickerDialog(title, initial) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.SelectedColor : (Color?)null;
        }


        // A row of the fields under the sliders: its label, at the right of the first column, and its box.
        private static void AddField(Grid fields, string label, FrameworkElement box)
        {
            int row = fields.RowDefinitions.Count;
            fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            double above = row == 0 ? 0 : 22;
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
                    hex.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
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
