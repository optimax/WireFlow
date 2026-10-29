using System.Collections.Generic;

namespace WireFlow
{
    public class FlowGroup : FlowElementBase
    {
        public IList<FlowElementBase> Children { get; set; } = new List<FlowElementBase>();
    }
}
