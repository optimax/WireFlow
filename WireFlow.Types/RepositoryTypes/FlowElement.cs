namespace WireFlow
{

    public class FlowElement: FlowElementBase
    {

        public static FlowElement Defaults { get; set; }

        private string stroke;
        private double strokeThickness;
        private double radiusX;
        private double radiusY;
        private string text;
        private string color;
        private string font;
        private double fontSize;
        private FlowFontStretch fontStretch;
        private int fontWeight;
        private double lineHeight;
        private FlowVerticalAlignment verticalAlignment;
        private FlowHorizontalAlignment horizontalAlignment;
        private FlowTextAlignment textAlignment;
        private FlowTextWrapping textWrapping;

        public string Stroke { get => stroke; set => Set(ref stroke, value); }
        public double StrokeThickness { get => strokeThickness; set => Set(ref strokeThickness, value); }
        public double RadiusX { get => radiusX; set => Set(ref radiusX, value); }
        public double RadiusY { get => radiusY; set => Set(ref radiusY, value); }
        public string Text { get => text; set => Set(ref text, value); }
        public string Color { get => color; set => Set(ref color, value); }
        public string Font { get => font; set => Set(ref font, value); }
        public double FontSize { get => fontSize; set => Set(ref fontSize, value); }
        public FlowFontStretch FontStretch { get => fontStretch; set => Set(ref fontStretch, value); }
        public int FontWeight { get => fontWeight; set => Set(ref fontWeight, value); }
        public double LineHeight { get => lineHeight; set => Set(ref lineHeight, value); }
        public FlowVerticalAlignment VerticalAlignment { get => verticalAlignment; set => Set(ref verticalAlignment, value); }
        public FlowHorizontalAlignment HorizontalAlignment { get => horizontalAlignment; set => Set(ref horizontalAlignment, value); }
        public FlowTextAlignment TextAlignment { get => textAlignment; set => Set(ref textAlignment, value); }
        public FlowTextWrapping TextWrapping { get => textWrapping; set => Set(ref textWrapping, value); }



        static FlowElement()
        {
            Defaults = new FlowElement()
            {
                Height = double.NaN,
                Width = double.NaN,
                HorizontalAlignment = FlowHorizontalAlignment.Center,
                VerticalAlignment = FlowVerticalAlignment.Center,
                TextAlignment = FlowTextAlignment.Center,
                TextWrapping = FlowTextWrapping.Wrap,
                FontWeight = 400,
                FontStretch = FlowFontStretch.Normal,
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
