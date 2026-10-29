using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WireFlow
{

    public class FlowElement: FlowElementBase
    {

        public static FlowElement Defaults { get; set; }

        public string Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public string Font { get; set; }
        public double FontSize { get; set; }
        public FontStretch FontStretch { get; set; }
        public int FontWeight { get; set; }
        public double LineHeight { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public TextWrapping TextWrapping { get; set; }



        static FlowElement()
        {
            Defaults = new FlowElement()
            {
                Height = double.NaN,
                Width = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = 400,
                FontStretch = FontStretches.Normal,
                Font = "Segoe UI",
                Color = "Black",
                FontSize = 12,
                StrokeThickness = 3.5,
                LineHeight = double.NaN
            };
        }


        public FlowElement(): this(Defaults)
        { }


        public FlowElement(FlowElement defaults)
        {
            if (defaults == null)
                return;
            Width = defaults.Width;
            Height = defaults.Height;
            Font = defaults.Font;
            FontWeight = defaults.FontWeight;
            FontStretch = defaults.FontStretch;
            FontSize = defaults.FontSize;
            Color = defaults.Color;
            LineHeight = defaults.LineHeight;
            //StrokeThickness = defaults.StrokeThickness;
            //TextAlignment = defaults.TextAlignment;
            //TextWrapping = defaults.TextWrapping;
            //HorizontalAlignment = defaults.HorizontalAlignment;
            //VerticalAlignment = defaults.VerticalAlignment;
        }

}
}
