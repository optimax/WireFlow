using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// Converts the DOM's values (WireFlow.Types, which uses no WPF type) to WPF's and back, for the WPF presentation. The
    /// enums have WPF's names and numbers; a font stretch is its OpenType width class.
    /// </summary>
    public static class WpfValues
    {
        public static HorizontalAlignment ToWpf(this FlowHorizontalAlignment value) => (HorizontalAlignment)value;
        public static VerticalAlignment ToWpf(this FlowVerticalAlignment value) => (VerticalAlignment)value;
        public static TextAlignment ToWpf(this FlowTextAlignment value) => (TextAlignment)value;
        public static TextWrapping ToWpf(this FlowTextWrapping value) => (TextWrapping)value;
        public static Stretch ToWpf(this FlowStretch value) => (Stretch)value;
        public static Orientation ToWpf(this FlowOrientation value) => (Orientation)value;

        /// <summary>The font stretch of a width class; Normal for a value outside 1 to 9.</summary>
        public static FontStretch ToWpf(this FlowFontStretch value) =>
            value >= FlowFontStretch.UltraCondensed && value <= FlowFontStretch.UltraExpanded ? FontStretch.FromOpenTypeStretch((int)value) : FontStretches.Normal;

        public static FlowHorizontalAlignment ToFlow(this HorizontalAlignment value) => (FlowHorizontalAlignment)value;
        public static FlowVerticalAlignment ToFlow(this VerticalAlignment value) => (FlowVerticalAlignment)value;
        public static FlowTextAlignment ToFlow(this TextAlignment value) => (FlowTextAlignment)value;
        public static FlowTextWrapping ToFlow(this TextWrapping value) => (FlowTextWrapping)value;
        public static FlowStretch ToFlow(this Stretch value) => (FlowStretch)value;
        public static FlowOrientation ToFlow(this Orientation value) => (FlowOrientation)value;
        public static FlowFontStretch ToFlow(this FontStretch value) => (FlowFontStretch)value.ToOpenTypeStretch();
    }
}
