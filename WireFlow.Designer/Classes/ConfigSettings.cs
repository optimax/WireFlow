using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace WireFlow
{
    public class ConfigSettings
    {
        // The settings file, in %APPDATA%\WireFlow
        public static string FileName =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Globals.CONFIG_APP_NAME, Globals.CONFIG_FILE_NAME);



        // The settings as the file keeps them, or the defaults when there is no file. The splash screen reads them before the window is
        // made (DesignerSplash), so this class stays apart from the window, whose type takes long to load.
        public static ConfigSettings Read()
        {
            string file = FileName;
            if (!File.Exists(file))
                return new ConfigSettings();
            return JsonConvert.DeserializeObject<ConfigSettings>(File.ReadAllText(file));
        }



        // Whether a size is kept for the window
        public bool HasWindowSize() => !double.IsNaN(Width) && !double.IsNaN(Height) && Width > 100 && Height > 100;



        // The place and size the window opens at (in device-independent pixels; maximized, the place it goes back to), as kept here, or
        // null where Windows places it: no place or size kept, or a place no longer on any screen (a screen taken away)
        public Rect? WindowBounds()
        {
            if (!HasWindowSize() || double.IsNaN(Left) || double.IsNaN(Top))
                return null;
            var visible = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            var bounds = new Rect(Left, Top, Width, Height);
            if (visible.IntersectsWith(bounds) && visible.Contains(new Point(Left + Math.Min(100, Width / 2), Top + 10)))
                return bounds;
            return null;
        }



        public bool Maximized { get; set; } = false;
        public double Left { get; set; } = double.NaN;
        public double Top { get; set; } = double.NaN;
        public double Width { get; set; } = double.NaN;
        public double Height { get; set; } = double.NaN;
        public string Background { get; set; } = Constants.DEFAULT_BACKGROUND;
        // System, Light, Dark or Classic (MaxServices.Core's ThemeChoice); anything else is System
        public string Theme { get; set; } = "System";
        // Tools > Options: open PreviousProject again when the Designer starts
        public bool ReopenPreviousProject { get; set; } = false;
        // Tools > Options: the Select tool again after each object drawn, as Esc chooses it
        public bool SelectAfterDrawing { get; set; } = false;
        // Tools > Options: the row of Properties with the keyboard focus highlighted, across the row
        public bool HighlightSelectedProperty { get; set; } = true;
        // Its color, #AARRGGBB; null for the Windows accent color, partly transparent
        public string SelectedPropertyColor { get; set; }
        // Tools > Options: the badge over the page in Navigate mode, its color and the color of its text, #AARRGGBB
        public string NavigateBadgeColor { get; set; } = "#FFE8590C";
        public string NavigateBadgeTextColor { get; set; } = "#FFFFFFFF";
        /// <summary>Whether the links on the pages are highlighted (View > Highlight Links).</summary>
        public bool HighlightLinks { get; set; } = false;
        /// <summary>The color of the highlight laid over links, #AARRGGBB, made partly transparent by <see cref="LinkHighlightOpacity"/>.</summary>
        public string LinkHighlightColor { get; set; } = "#FF1E88E5";
        /// <summary>How opaque the highlight laid over links is, from 0 to 1.</summary>
        public double LinkHighlightOpacity { get; set; } = 0.35;
        // The project open when the settings were last saved, as the Designer closed; null when none was
        public string PreviousProject { get; set; }
        // Its tabs then, in their order, and the index of the one in front (-1: none), reopened with it
        public List<TabSetting> PreviousTabs { get; set; } = new List<TabSetting>();
        public int PreviousTab { get; set; } = -1;
        // The colors last chosen in the color dialog, the latest first, as #AARRGGBB (ColorPickerDialog.RecentColors)
        public List<string> RecentColors { get; set; } = new List<string>();

        /// <summary>The docking layout of the panes as the Designer was last closed (XML, <see cref="DockLayoutSerializer"/>); null for the one built in.</summary>
        public string DockLayout { get; set; }
    }


    /// <summary>An open tab, kept in the settings: what it edits (DesignerNavigatorDocument's PageKind, LayoutKind or PartKind) and its name.</summary>
    public class TabSetting
    {
        public string Kind { get; set; }
        public string Name { get; set; }
    }
}
