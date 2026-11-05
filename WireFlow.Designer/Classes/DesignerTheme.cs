using System;
using System.Windows;
using MaxServices.Core.Themes;

namespace WireFlow
{
    /// <summary>
    /// The Designer's themes: System, Light, Dark or Classic, through MaxServices.Core's AppTheme, as in PDFTools.
    /// The Designer's own brush keys and the AvalonDock Metro theme's base colors are kept in the application's
    /// resources as the palette's brushes for the current theme, so that XAML refers to them by DynamicResource.
    /// </summary>
    public static class DesignerTheme
    {
        private static readonly (string Key, ComponentResourceKey Palette)[] Aliases =
        {
            ("NormalColorBrush", Palette.AppBackground),
            ("ListBackgroundBrush", Palette.PaneBackground),
            ("ToolbarPanelBackgroundBrush", Palette.PaneHeader),
            ("DarkDarkDarkBrush", Palette.SectionCaption),
            ("LightLightColorBrush", Palette.Text),
            ("LightColorBrush", Palette.SecondaryText),
            ("DesignerBackgroundBrush", Palette.PreviewBackground),
            ("PageFrameBrush", Palette.PageFrame),

            // Metro's 3 and 4 are its blue accents, which stay as they are
            ("AvalonDock_ThemeMetro_BaseColor1", Palette.PaneBackground),
            ("AvalonDock_ThemeMetro_BaseColor2", Palette.Text),
            ("AvalonDock_ThemeMetro_BaseColor5", Palette.MutedText),
            ("AvalonDock_ThemeMetro_BaseColor6", Palette.FaintText),
            ("AvalonDock_ThemeMetro_BaseColor7", Palette.PaneHeader),
            ("AvalonDock_ThemeMetro_SplitterBorder", Palette.ChromeBorder),
        };

        private static bool _subscribed;

        public static void Apply(ThemeChoice choice)
        {
            if (!_subscribed)
            {
                AppTheme.Changed += (s, e) => UpdateAliases();
                _subscribed = true;
            }
            AppTheme.Apply(choice);
            UpdateAliases();
        }

        /// <summary>
        /// The choice named by a setting; anything else is System.
        /// </summary>
        public static ThemeChoice Parse(string value)
        {
            return AppTheme.TryParse(value, out var choice) ? choice : ThemeChoice.System;
        }

        private static void UpdateAliases()
        {
            // TryFindResource, as the light palette is partly in Core's theme dictionary rather than in the resources
            var app = Application.Current;
            foreach (var (key, palette) in Aliases)
                app.Resources[key] = app.TryFindResource(palette);
        }
    }
}
