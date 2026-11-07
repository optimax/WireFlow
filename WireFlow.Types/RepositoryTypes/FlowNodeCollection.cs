using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WireFlow
{
    /// <summary>
    /// The children of a DOM node (a page's layers, a layer's or group's elements): it tells of each change
    /// (CollectionChanged), and makes the node owning it the Parent of each node it holds.
    /// </summary>
    public class FlowNodeCollection<T> : ObservableCollection<T> where T : FlowBase
    {
        private readonly FlowBase owner;


        public FlowNodeCollection(FlowBase owner)
        {
            this.owner = owner;
        }


        /// <summary>Puts the nodes of a list in place of those it holds (a list it is given as a whole, as the JSON reader may).</summary>
        public void ReplaceWith(IEnumerable<T> nodes)
        {
            if (ReferenceEquals(nodes, this))
                return;
            var list = nodes == null ? new List<T>() : new List<T>(nodes);
            Clear();
            foreach (var node in list)
                Add(node);
        }


        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (item != null)
                item.Parent = owner;
        }


        protected override void SetItem(int index, T item)
        {
            var old = this[index];
            base.SetItem(index, item);
            Release(old);
            if (item != null)
                item.Parent = owner;
        }


        protected override void RemoveItem(int index)
        {
            var old = this[index];
            base.RemoveItem(index);
            Release(old);
        }


        protected override void ClearItems()
        {
            var nodes = new List<T>(this);
            base.ClearItems();
            foreach (var node in nodes)
                Release(node);
        }


        // A node taken out is no longer this owner's, unless it is still here; one already put elsewhere keeps its new parent
        private void Release(T node)
        {
            if (node != null && node.Parent == owner && !Contains(node))
                node.Parent = null;
        }
    }
}
