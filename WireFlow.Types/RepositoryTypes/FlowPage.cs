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
        // Its own background color, or null when it takes that of its nearest layout that has one, else the project's
        public string Background { get; set; }

        public bool IsMaster { get; set; }
        public IList<FlowLayer> Layers { get; set; } = new List<FlowLayer>();



        ////TODO: Remove
        //public IEnumerable<FlowLink> Links { get; set; }
    }
}
