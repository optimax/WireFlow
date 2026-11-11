using System.Collections.Generic;

namespace WireFlow
{
    public class ConfigSettings
    {
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
