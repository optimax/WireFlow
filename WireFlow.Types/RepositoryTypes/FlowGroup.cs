using System.Collections.Generic;

namespace WireFlow
{
    public class FlowGroup : FlowElementBase
    {
        private readonly FlowNodeCollection<FlowElementBase> children;


        public FlowGroup()
        {
            children = new FlowNodeCollection<FlowElementBase>(this);
        }


        /// <summary>Its elements, bottom first; a list set here replaces what the group holds.</summary>
        public IList<FlowElementBase> Children
        {
            get => children;
            set => children.ReplaceWith(value);
        }
    }
}
