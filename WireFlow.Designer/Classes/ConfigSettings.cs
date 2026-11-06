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
        // The project open when the settings were last saved, as the Designer closed; null when none was
        public string PreviousProject { get; set; }
    }
}
