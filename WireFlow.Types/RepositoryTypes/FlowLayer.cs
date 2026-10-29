using System.Collections.Generic;

namespace WireFlow
{
    public class FlowLayer: FlowBase
    {
        public bool Hidden { get; set; }
        public bool Locked { get; set; }


        public IList<FlowElementBase> Elements { get; set; } = new List<FlowElementBase>();

    }
}
