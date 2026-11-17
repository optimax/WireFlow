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
        private bool locked;

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

        /// <summary>
        /// Whether the element's properties, but this one, are kept from being changed in the Designer. A file written
        /// before it has none, which is not locked.
        /// </summary>
        public bool Locked { get => locked; set => Set(ref locked, value); }

        /// <summary>
        /// Whether the element is kept from being changed: it is locked itself, or something holding it is: a group, a
        /// stack panel, a panel, or its layer.
        /// </summary>
        [System.ComponentModel.Browsable(false), System.Runtime.Serialization.IgnoreDataMember]
        public bool IsLocked
        {
            get
            {
                for (FlowBase node = this; node != null; node = node.Parent)
                    if ((node as FlowElementBase)?.Locked == true || (node as FlowLayer)?.Locked == true)
                        return true;
                return false;
            }
        }

        /// <summary>Where the panel holding the element places it; an element that is not in a panel keeps its own place.</summary>
        public FlowAlignment Alignment { get => alignment; set => Set(ref alignment, value); }


        protected FlowElementBase()
        {
            Width = double.NaN;
            Height = double.NaN;
        }
    }
}
