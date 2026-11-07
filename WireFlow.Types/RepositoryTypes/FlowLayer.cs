using System.Collections.Generic;

namespace WireFlow
{
    public class FlowLayer: FlowBase
    {
        private bool hidden;
        private bool locked;
        private readonly FlowNodeCollection<FlowElementBase> elements;


        public FlowLayer()
        {
            elements = new FlowNodeCollection<FlowElementBase>(this);
        }


        public bool Hidden { get => hidden; set => Set(ref hidden, value); }
        public bool Locked { get => locked; set => Set(ref locked, value); }

        /// <summary>Its elements, bottom first; a list set here replaces what the layer holds.</summary>
        public IList<FlowElementBase> Elements
        {
            get => elements;
            set => elements.ReplaceWith(value);
        }
    }
}
