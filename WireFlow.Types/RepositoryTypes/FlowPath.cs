using System.Collections.Generic;

namespace WireFlow
{
    /// <summary>
    /// Where a layer or an element lies in its page or layout: its layer's index among the page's layers, then each
    /// element's index in what holds it (a layer, a group, a stack panel or a panel). The same place finds the same node in
    /// another copy of the page, as one read again from the project or held by another tab.
    /// </summary>
    public static class FlowPath
    {
        /// <summary>
        /// The place of a layer or an element in the page or layout holding it, which <paramref name="page"/> gives; null,
        /// with no page, for a node that is neither or that no page holds (an element of a part).
        /// </summary>
        public static List<int> Of(FlowBase node, out FlowPage page)
        {
            page = null;
            if (!(node is FlowLayer || node is FlowElementBase))
                return null;
            var path = new List<int>();
            for (var n = node; n != null; n = n.Parent)
            {
                int index;
                switch (n.Parent)
                {
                    case FlowPage holder when n is FlowLayer layer:
                        index = holder.Layers.IndexOf(layer);
                        break;
                    case FlowLayer layer when n is FlowElementBase element:
                        index = layer.Elements.IndexOf(element);
                        break;
                    case FlowGroup group when n is FlowElementBase element:
                        index = group.Children.IndexOf(element);
                        break;
                    case null when n is FlowPage root:
                        page = root;
                        return path;
                    default:
                        return null;
                }
                if (index < 0)
                    return null;
                path.Insert(0, index);
            }
            return null;
        }


        /// <summary>The layer or element at a place in a page or layout; null where it has none.</summary>
        public static FlowBase At(FlowPage page, IReadOnlyList<int> path)
        {
            if (page == null || path == null || path.Count == 0)
                return null;
            FlowBase node = page;
            foreach (int i in path)
            {
                node = node switch
                {
                    FlowPage p when i >= 0 && i < p.Layers.Count => p.Layers[i],
                    FlowLayer l when i >= 0 && i < l.Elements.Count => l.Elements[i],
                    FlowGroup g when i >= 0 && i < g.Children.Count => g.Children[i],
                    _ => null
                };
                if (node == null)
                    return null;
            }
            return node;
        }
    }
}
