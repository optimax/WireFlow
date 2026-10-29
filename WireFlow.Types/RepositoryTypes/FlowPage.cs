using System;
using System.Collections.Generic;


namespace WireFlow
{
    public class FlowPage: FlowBase
    {
        //public string Title { get; set; }

        public string Master { get; set; }
        public double Width { get; set; } = Double.NaN;
        public double Height { get; set; } = Double.NaN;

        public bool IsMaster { get; set; }
        public IList<FlowLayer> Layers { get; set; } = new List<FlowLayer>();



        ////TODO: Remove
        //public IEnumerable<FlowLink> Links { get; set; }
    }
}
