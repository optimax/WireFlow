namespace WireFlow
{
    public abstract class FlowElementBase: FlowBase
    {
        private double left;
        private double top;
        private double width;
        private double height;
        private string href;
        private string fill;
        private double transparency;
        private FlowAlignment alignment;
        private bool visible = true;

        public double Left { get => left; set => Set(ref left, value); }
        public double Top { get => top; set => Set(ref top, value); }
        public double Width { get => width; set => Set(ref width, value); }
        public double Height { get => height; set => Set(ref height, value); }
        public string Href { get => href; set => Set(ref href, value); }
        public string Fill { get => fill; set => Set(ref fill, value); }
        public double Transparency { get => transparency; set => Set(ref transparency, value); }

        /// <summary>
        /// Whether the element shows on its page. One that does not is not drawn and takes no room in a stack panel or a
        /// panel; it stays in the page and in the Outline. A file written before it has none, which is visible.
        /// </summary>
        public bool Visible { get => visible; set => Set(ref visible, value); }

        /// <summary>Where the panel holding the element places it; an element that is not in a panel keeps its own place.</summary>
        public FlowAlignment Alignment { get => alignment; set => Set(ref alignment, value); }


        protected FlowElementBase()
        {
            Width = double.NaN;
            Height = double.NaN;
        }
    }
}
