using System;
using System.IO;
using Newtonsoft.Json;

namespace WireFlow
{
    /// <summary>
    /// The Browser's settings, in %APPDATA%\WireFlow beside the Designer's.
    /// </summary>
    public class BrowserSettings
    {
        // System, Light, Dark or Classic (MaxServices.Core's ThemeChoice); anything else is System
        public string Theme { get; set; } = "System";


        public static string FileName =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WireFlow", "WireFlow.Browser.config");


        public static BrowserSettings Load()
        {
            try
            {
                if (File.Exists(FileName))
                    return JsonConvert.DeserializeObject<BrowserSettings>(File.ReadAllText(FileName)) ?? new BrowserSettings();
            }
            catch (Exception)
            {
                // An unreadable file gives the defaults; the next Save replaces it
            }
            return new BrowserSettings();
        }


        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            File.WriteAllText(FileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
