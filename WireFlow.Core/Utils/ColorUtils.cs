using System;
using System.Windows.Media;

namespace WireFlow
{
    public static class ColorUtils
    {

        public static Color StringToColor(string s, string def = "Black")
        {
            var convertFromString = ColorConverter.ConvertFromString(String.IsNullOrWhiteSpace(s) ? def : s);
            if (convertFromString != null)
                return (Color)convertFromString;
            return default(Color);
        }


        public static Brush StringToBrush(string s, string def)
        {
            Color color = StringToColor(s, def);
            if (color == default(Color))
                return null;
            return new SolidColorBrush((Color)color);
        }


        public static Brush StringToBrush(string s, Brush def = null)
        {
            Color color = StringToColor(s);
            if (color == default(Color))
                return def;
            return new SolidColorBrush((Color)color);
        }

    }
}
