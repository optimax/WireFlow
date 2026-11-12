using System;
using System.Collections.Generic;
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

        // The colors offered for the Navigate mode badge and its text, opaque and vivid, so that it stands out on any page
        private readonly List<string> badgeColors = new List<string> { "#FFE8590C", "#FFD62828", "#FF1E88E5", "#FF2E9E44", "#FF8E44AD", "#FF00897B", "#FFF2B705", "#FF222222" };
        private readonly List<string> badgeTextColors = new List<string> { "#FFFFFFFF", "#FF000000" };

        // The colors and opacities offered for the highlight laid over links
        private readonly List<string> linkColors = new List<string> { "#FF1E88E5", "#FFF2B705", "#FF2E9E44", "#FFE8590C", "#FFD62828", "#FF8E44AD", "#FF00897B", "#FF808080" };
        private readonly List<double> linkOpacities = new List<double> { 0.2, 0.3, 0.35, 0.4, 0.5, 0.6 };


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


        public DesignerOptionsDialog(ConfigSettings settings)
        {
            ReopenPreviousProject = settings.ReopenPreviousProject;
            SelectAfterDrawing = settings.SelectAfterDrawing;
            HighlightSelectedProperty = settings.HighlightSelectedProperty;
            SelectedPropertyColor = settings.SelectedPropertyColor;
            NavigateBadgeColor = settings.NavigateBadgeColor;
            NavigateBadgeTextColor = settings.NavigateBadgeTextColor;
            LinkHighlightColor = settings.LinkHighlightColor;
            LinkHighlightOpacity = settings.LinkHighlightOpacity;
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
            Choice(navigate, "NavigateBadgeColor", "Indicator color",
                ColorNames(badgeColors, new List<string> { "Orange", "Red", "Blue", "Green", "Purple", "Teal", "Yellow", "Charcoal" }, settings.NavigateBadgeColor, out index), index,
                "The color of the badge that hangs from the top of each page while the Designer is in Navigate mode.");
            Choice(navigate, "NavigateBadgeTextColor", "Indicator text color",
                ColorNames(badgeTextColors, new List<string> { "White", "Black" }, settings.NavigateBadgeTextColor, out index), index,
                "The color of the words \"Navigate Mode\" on the badge.");
            Choice(navigate, "LinkHighlightColor", "Link highlight color",
                ColorNames(linkColors, new List<string> { "Blue", "Yellow", "Green", "Orange", "Red", "Purple", "Teal", "Gray" }, settings.LinkHighlightColor, out index), index,
                "The color laid over every link on the pages while View > Highlight Links is on.");
            var opacityNames = linkOpacities.ConvertAll(o => $"{o * 100:0}%");
            index = linkOpacities.FindIndex(o => Math.Abs(o - settings.LinkHighlightOpacity) < 0.001);
            if (index < 0)
            {
                // An opacity typed into the settings file, offered as it is
                linkOpacities.Add(settings.LinkHighlightOpacity);
                opacityNames.Add($"Custom ({settings.LinkHighlightOpacity * 100:0.#}%)");
                index = linkOpacities.Count - 1;
            }
            Choice(navigate, "LinkHighlightOpacity", "Link highlight opacity", opacityNames, index,
                "How much of the link's color covers the link: the page shows through the rest.");
            Finish();
        }


        protected override void Apply()
        {
            ReopenPreviousProject = IsChecked("ReopenPreviousProject");
            SelectAfterDrawing = IsChecked("SelectAfterDrawing");
            HighlightSelectedProperty = IsChecked("HighlightSelectedProperty");
            int choice = ChoiceOf("SelectedPropertyColor");
            SelectedPropertyColor = choice >= 0 && choice < highlightColors.Count ? highlightColors[choice] : null;
            choice = ChoiceOf("NavigateBadgeColor");
            NavigateBadgeColor = choice >= 0 && choice < badgeColors.Count ? badgeColors[choice] : badgeColors[0];
            choice = ChoiceOf("NavigateBadgeTextColor");
            NavigateBadgeTextColor = choice >= 0 && choice < badgeTextColors.Count ? badgeTextColors[choice] : badgeTextColors[0];
            choice = ChoiceOf("LinkHighlightColor");
            LinkHighlightColor = choice >= 0 && choice < linkColors.Count ? linkColors[choice] : linkColors[0];
            choice = ChoiceOf("LinkHighlightOpacity");
            LinkHighlightOpacity = choice >= 0 && choice < linkOpacities.Count ? linkOpacities[choice] : 0.35;
        }


        // The names of the colors offered, and the index of the one set; a color set that is not offered (typed into the
        // settings file) is added to the colors and offered as Custom
        private static List<string> ColorNames(List<string> colors, List<string> names, string color, out int index)
        {
            index = colors.FindIndex(c => string.Equals(c, color, StringComparison.OrdinalIgnoreCase));
            if (index < 0 && !string.IsNullOrWhiteSpace(color))
            {
                colors.Add(color);
                names.Add($"Custom ({color})");
                index = colors.Count - 1;
            }
            return names;
        }
    }
}
