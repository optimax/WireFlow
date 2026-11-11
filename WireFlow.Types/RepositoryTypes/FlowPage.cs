using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

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


        /// <summary>
        /// The page's key in the project, which the repository gives it and which never changes: what the project's page order,
        /// tabs and history name it by, whatever its name. Not shown, and not written into the page's JSON (the repository keeps
        /// it beside it); null for a page that is not a project's.
        /// </summary>
        [Browsable(false), IgnoreDataMember]
        public string Key { get; set; }

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
