namespace WireFlow
{
    public abstract class FlowElementBase: FlowBase
    {
        public double Left { get; set; }
        public double Top { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public string Href { get; set; }


        public string Fill { get; set; }
        public double Transparency { get; set; }


        protected FlowElementBase()
        {
            Width = double.NaN;
            Height = double.NaN;
        }

    }
}
