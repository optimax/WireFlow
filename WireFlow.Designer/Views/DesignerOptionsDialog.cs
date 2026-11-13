using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MaxServices.Core.Themes;
using OptimaxTools.Options;

namespace WireFlow.Views
{
    /// <summary>
    /// Tools > Options: the Options dialog of MarkdownTools' and PDFTools' programs (OptimaxTools.Options), in the Designer's
    /// theme, with its groups of options on the left and the chosen group's on the right. It changes nothing itself: OK sets
    /// its results, which the main window applies and saves.
    /// </summary>
    internal sealed class DesignerOptionsDialog : OptionsDialog
    {
        // The highlight colors offered, in the list's order: the Windows accent (null), then named ones, partly transparent so
        // that the text shows through on light and dark grids alike (and a custom one, when typed into the settings file)
        private readonly List<string> highlightColors = new List<string> { null, "#603399FF", "#6032B45A", "#60FF9933", "#608A4FC8", "#60808080" };

        // The opacities offered for the highlight laid over links
        private readonly List<double> linkOpacities = new List<double> { 0.2, 0.3, 0.35, 0.4, 0.5, 0.6 };

        // The color of each color row, by its key, #AARRGGBB, as its button shows it
        private readonly Dictionary<string, string> colors = new Dictionary<string, string>();

        // Each color row's list, made for its words and its place, and the button that takes the list's place once the
        // dialog has laid out its rows (Finish)
        private readonly List<(ComboBox List, Button Button)> colorButtons = new List<(ComboBox, Button)>();


        /// <summary>Whether the project open when the Designer closed opens again when it starts, as OK left it.</summary>
        public bool ReopenPreviousProject { get; private set; }

        /// <summary>Whether the Select tool is chosen again after each object drawn, as OK left it.</summary>
        public bool SelectAfterDrawing { get; private set; }

        /// <summary>Whether the row of Properties with the keyboard focus is highlighted, as OK left it.</summary>
        public bool HighlightSelectedProperty { get; private set; }

        /// <summary>The highlight's color, #AARRGGBB, or null for the Windows accent, as OK left it.</summary>
        public string SelectedPropertyColor { get; private set; }

        /// <summary>The color of the Navigate mode badge, #AARRGGBB, as OK left it.</summary>
        public string NavigateBadgeColor { get; private set; }

        /// <summary>The color of the badge's text, #AARRGGBB, as OK left it.</summary>
        public string NavigateBadgeTextColor { get; private set; }

        /// <summary>The color of the highlight laid over links, #AARRGGBB, as OK left it.</summary>
        public string LinkHighlightColor { get; private set; }

        /// <summary>How opaque the highlight laid over links is, from 0 to 1, as OK left it.</summary>
        public double LinkHighlightOpacity { get; private set; }


        /// <summary>
        /// The options of the settings; the Navigate mode colors are the project's when one is open (null: none), its own or, where
        /// it has none, the Designer's, which with no project open are the colors new projects start with.
        /// </summary>
        public DesignerOptionsDialog(ConfigSettings settings, FlowProject project = null)
        {
            ReopenPreviousProject = settings.ReopenPreviousProject;
            SelectAfterDrawing = settings.SelectAfterDrawing;
            HighlightSelectedProperty = settings.HighlightSelectedProperty;
            SelectedPropertyColor = settings.SelectedPropertyColor;
            NavigateBadgeColor = project?.NavigateBadgeColor ?? settings.NavigateBadgeColor;
            NavigateBadgeTextColor = project?.NavigateBadgeTextColor ?? settings.NavigateBadgeTextColor;
            LinkHighlightColor = project?.LinkHighlightColor ?? settings.LinkHighlightColor;
            LinkHighlightOpacity = project?.LinkHighlightOpacity ?? settings.LinkHighlightOpacity;
            // In the theme's colors, as the color dialog
            SetResourceReference(BackgroundProperty, "NormalColorBrush");
            SetResourceReference(ForegroundProperty, "LightLightColorBrush");
            SourceInitialized += (s, e) => AppTheme.ApplyTitleBar(this);

            var files = Page("Files");
            Check(files, "ReopenPreviousProject", "Reopen the previous project at startup", settings.ReopenPreviousProject).ToolTip =
                "The project open when the Designer was last closed opens again when it starts.";
            var drawing = Page("Drawing");
            Check(drawing, "SelectAfterDrawing", "Return to the Select tool after drawing an object", settings.SelectAfterDrawing).ToolTip =
                "Once an object is drawn, the tool is Select again, as Esc makes it; otherwise the tool stays for drawing more.";

            var properties = Page("Properties");
            Check(properties, "HighlightSelectedProperty", "Highlight the row being edited", settings.HighlightSelectedProperty).ToolTip =
                "The row of Properties with the keyboard focus is highlighted across its whole width.";
            var names = new List<string> { "The Windows accent color", "Blue", "Green", "Orange", "Purple", "Gray" };
            int index = highlightColors.FindIndex(c => string.Equals(c, settings.SelectedPropertyColor, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                // A color typed into the settings file, offered as it is
                highlightColors.Add(settings.SelectedPropertyColor);
                names.Add($"Custom ({settings.SelectedPropertyColor})");
                index = highlightColors.Count - 1;
            }
            Choice(properties, "SelectedPropertyColor", "Highlight color", names, index,
                "The color of the highlight, partly transparent so that the text shows on it in every theme.");

            var navigate = Page("Navigate Mode");
            ColorRow(navigate, "NavigateBadgeColor", "Indicator color", NavigateBadgeColor, Color.FromRgb(0xE8, 0x59, 0x0C),
                "The color of the badge that hangs from the top of each page while the Designer is in Navigate mode.");
            ColorRow(navigate, "NavigateBadgeTextColor", "Indicator text color", NavigateBadgeTextColor, Colors.White,
                "The color of the words \"Navigate Mode\" on the badge.");
            ColorRow(navigate, "LinkHighlightColor", "Link highlight color", LinkHighlightColor, Color.FromRgb(0x1E, 0x88, 0xE5),
                "The color laid over every link on the pages while View > Highlight Links is on.");
            var opacityNames = linkOpacities.ConvertAll(o => $"{o * 100:0}%");
            index = linkOpacities.FindIndex(o => Math.Abs(o - LinkHighlightOpacity) < 0.001);
            if (index < 0)
            {
                // An opacity typed into the settings file, offered as it is
                linkOpacities.Add(LinkHighlightOpacity);
                opacityNames.Add($"Custom ({LinkHighlightOpacity * 100:0.#}%)");
                index = linkOpacities.Count - 1;
            }
            Choice(navigate, "LinkHighlightOpacity", "Link highlight opacity", opacityNames, index,
                "How much of the link's color covers the link: the page shows through the rest.");
            // Whose colors these are
            navigate.Children.Add(new TextBlock
            {
                Text = project != null
                    ? "These colors are the open project's, kept in its file, so that they can suit its pages. With no project open, Options sets the colors new projects start with."
                    : "With no project open, these are the colors new projects start with, and those of projects that have none of their own.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.75,
                Margin = new Thickness(0, 12, 0, 0)
            });
            Finish();
            PlaceColorButtons();
        }


        protected override void Apply()
        {
            ReopenPreviousProject = IsChecked("ReopenPreviousProject");
            SelectAfterDrawing = IsChecked("SelectAfterDrawing");
            HighlightSelectedProperty = IsChecked("HighlightSelectedProperty");
            int choice = ChoiceOf("SelectedPropertyColor");
            SelectedPropertyColor = choice >= 0 && choice < highlightColors.Count ? highlightColors[choice] : null;
            NavigateBadgeColor = colors["NavigateBadgeColor"];
            NavigateBadgeTextColor = colors["NavigateBadgeTextColor"];
            LinkHighlightColor = colors["LinkHighlightColor"];
            choice = ChoiceOf("LinkHighlightOpacity");
            LinkHighlightOpacity = choice >= 0 && choice < linkOpacities.Count ? linkOpacities[choice] : 0.35;
        }


        // A row of a color: its words, and a button showing the color as a swatch and as #AARRGGBB, which opens the color
        // dialog (its swatches, and the colors chosen last) on it; the color the button shows is the row's. The row is made
        // as a list's, so that its words and its place are those of the other rows, and the button takes the list's place.
        private void ColorRow(Panel panel, string key, string text, string color, Color fallback, string toolTip)
        {
            Color shown = ColorOf(color) ?? fallback;
            colors[key] = ToText(shown);
            var list = Choice(panel, key, text, new List<string> { colors[key] }, 0, toolTip);
            var swatch = new Border { Width = 16, Height = 16, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
            swatch.SetBinding(Border.BorderBrushProperty, new Binding(nameof(Foreground)) { Source = this });
            var words = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(swatch);
            content.Children.Add(words);
            var button = new Button { Content = content, HorizontalContentAlignment = HorizontalAlignment.Left, ToolTip = list.ToolTip };
            AutomationProperties.SetName(button, text);
            void Show(Color c)
            {
                colors[key] = ToText(c);
                swatch.Background = new SolidColorBrush(c);
                words.Text = colors[key];
            }
            Show(shown);
            button.Click += (s, e) =>
            {
                Color? chosen = ColorPickerDialog.Show(this, text, ColorOf(colors[key]) ?? fallback);
                if (chosen != null)
                    Show(chosen.Value);
            };
            colorButtons.Add((list, button));
        }


        // Puts each color row's button where its list is, at its size, and hides the list
        private void PlaceColorButtons()
        {
            foreach (var (list, button) in colorButtons)
            {
                if (!(list.Parent is Panel row))
                    continue;
                button.Width = list.Width;
                button.MinWidth = list.MinWidth;
                button.MaxWidth = list.MaxWidth;
                button.MinHeight = list.MinHeight;
                button.Margin = list.Margin;
                button.HorizontalAlignment = list.HorizontalAlignment;
                button.VerticalAlignment = list.VerticalAlignment;
                Grid.SetRow(button, Grid.GetRow(list));
                Grid.SetColumn(button, Grid.GetColumn(list));
                Grid.SetColumnSpan(button, Grid.GetColumnSpan(list));
                DockPanel.SetDock(button, DockPanel.GetDock(list));
                list.Visibility = Visibility.Collapsed;
                row.Children.Insert(row.Children.IndexOf(list) + 1, button);
            }
        }


        private static string ToText(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";


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
    }
}
