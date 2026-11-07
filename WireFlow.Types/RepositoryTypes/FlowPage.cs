using System;
using System.Collections.Generic;

namespace WireFlow
{
    public class FlowPage: FlowBase
    {
        private string master;
        private double width = Double.NaN;
        private double height = Double.NaN;
        private string background;
        private bool isMaster;
        private readonly FlowNodeCollection<FlowLayer> layers;


        public FlowPage()
        {
            layers = new FlowNodeCollection<FlowLayer>(this);
        }


        //public string Title { get; set; }
        public string Master { get => master; set => Set(ref master, value); }
        public double Width { get => width; set => Set(ref width, value); }
        public double Height { get => height; set => Set(ref height, value); }

        // Its own background color, or null when it takes that of its nearest layout that has one, else the project's
        public string Background { get => background; set => Set(ref background, value); }

        public bool IsMaster { get => isMaster; set => Set(ref isMaster, value); }

        /// <summary>Its layers, bottom first; a list set here replaces what the page holds.</summary>
        public IList<FlowLayer> Layers
        {
            get => layers;
            set => layers.ReplaceWith(value);
        }

        ////TODO: Remove
        //public IEnumerable<FlowLink> Links { get; set; }
    }
}
