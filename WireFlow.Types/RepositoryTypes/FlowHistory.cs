using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WireFlow
{
    /// <summary>
    /// The Undo/Redo history of a DOM tree (a page or layout): it watches every node of the tree, records each change of a property
    /// and of a list of children, and undoes and redoes them. Changes are gathered into steps: those between BeginStep and EndStep
    /// make one, and any others make one each time CommitStep is called. The history knows nothing of the code making the changes,
    /// so every edit made to the DOM, by whatever means, can be undone.
    /// </summary>
    public class FlowHistory
    {
        private readonly FlowBase root;
        private readonly Dictionary<FlowBase, int> watched = new Dictionary<FlowBase, int>();
        private readonly List<List<Change>> undoSteps = new List<List<Change>>();
        private readonly List<List<Change>> redoSteps = new List<List<Change>>();
        private List<Change> openStep;
        private int stepDepth;
        private bool replaying;
        private bool detached;


        public FlowHistory(FlowBase root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
            Watch(root);
        }


        /// <summary>Raised when a change is recorded, and when a step is undone or redone, or the history cleared.</summary>
        public event EventHandler Changed;

        /// <summary>Raised when a node is put in the tree by an edit (not by undoing or redoing one), after it is recorded, so that
        /// changes made to it then are recorded in the same step.</summary>
        public event Action<FlowBase> NodeAdded;

        /// <summary>Raised once a step is undone or redone, for each change of a property in it, in the order they were made
        /// again: the node, the property's name, and its value before and after the undo or redo. Changes a handler makes to
        /// the tree are recorded as a new edit.</summary>
        public event Action<FlowBase, string, object, object> PropertyReplayed;

        public FlowBase Root => root;

        /// <summary>Whether there is a step to undo, the step still open included.</summary>
        public bool CanUndo => undoSteps.Count > 0 || openStep != null;

        public bool CanRedo => redoSteps.Count > 0;

        /// <summary>Whether changes are recorded that no step holds yet, outside BeginStep and EndStep: CommitStep makes them one.</summary>
        public bool HasUncommittedChanges => openStep != null && stepDepth == 0;

        /// <summary>Whether the history is undoing or redoing a step: the changes it makes then are not recorded.</summary>
        public bool IsReplaying => replaying;


        /// <summary>Starts a step: the changes until the matching EndStep make one step. Steps may nest; the outermost counts.</summary>
        public void BeginStep()
        {
            stepDepth++;
        }


        /// <summary>Ends a step started by BeginStep.</summary>
        public void EndStep()
        {
            if (stepDepth == 0)
                return;
            stepDepth--;
            if (stepDepth == 0)
                CommitStep();
        }


        /// <summary>Makes the changes recorded so far one step, unless a step started by BeginStep is open.</summary>
        public void CommitStep()
        {
            if (stepDepth > 0 || openStep == null)
                return;
            undoSteps.Add(openStep);
            openStep = null;
        }


        /// <summary>Undoes the last step.</summary>
        public bool Undo()
        {
            stepDepth = 0;
            CommitStep();
            if (undoSteps.Count == 0)
                return false;
            var step = undoSteps[undoSteps.Count - 1];
            undoSteps.RemoveAt(undoSteps.Count - 1);
            Replay(() =>
            {
                for (var i = step.Count - 1; i >= 0; i--)
                    step[i].Undo();
            });
            redoSteps.Add(step);
            OnChanged();
            for (var i = step.Count - 1; i >= 0; i--)
                if (step[i] is PropertyChange change)
                    PropertyReplayed?.Invoke(change.Node, change.Name, change.NewValue, change.OldValue);
            return true;
        }


        /// <summary>Redoes the last step undone.</summary>
        public bool Redo()
        {
            stepDepth = 0;
            CommitStep();
            if (redoSteps.Count == 0)
                return false;
            var step = redoSteps[redoSteps.Count - 1];
            redoSteps.RemoveAt(redoSteps.Count - 1);
            Replay(() =>
            {
                foreach (var change in step)
                    change.Redo();
            });
            undoSteps.Add(step);
            OnChanged();
            foreach (var redone in step)
                if (redone is PropertyChange change)
                    PropertyReplayed?.Invoke(change.Node, change.Name, change.OldValue, change.NewValue);
            return true;
        }


        /// <summary>Forgets every step, as when the tree is saved.</summary>
        public void Clear()
        {
            undoSteps.Clear();
            redoSteps.Clear();
            openStep = null;
            stepDepth = 0;
            OnChanged();
        }


        /// <summary>Stops watching the tree; the history records nothing more.</summary>
        public void Detach()
        {
            if (detached)
                return;
            detached = true;
            foreach (var node in new List<FlowBase>(watched.Keys))
            {
                node.PropertyChanged -= NodePropertyChanged;
                foreach (var list in ChildLists(node))
                    list.CollectionChanged -= ChildrenChanged;
            }
            watched.Clear();
        }


        // The lists of children of a node: a page's layers, a layer's elements, a group's (a stack panel's, a part's) children
        private static IEnumerable<IFlowNodeCollection> ChildLists(FlowBase node)
        {
            switch (node)
            {
                case FlowPage page:
                    if (page.Layers is IFlowNodeCollection layers)
                        yield return layers;
                    break;
                case FlowLayer layer:
                    if (layer.Elements is IFlowNodeCollection elements)
                        yield return elements;
                    break;
                case FlowGroup group:
                    if (group.Children is IFlowNodeCollection children)
                        yield return children;
                    break;
            }
        }


        // Watches a node and the nodes under it. A node may be counted twice for a moment, when it is put in its new place before it is
        // taken from its old one, so each watch is counted and the node let go only when the last is.
        private void Watch(FlowBase node)
        {
            if (node == null || detached)
                return;
            watched.TryGetValue(node, out var count);
            watched[node] = count + 1;
            if (count == 0)
            {
                node.PropertyChanged += NodePropertyChanged;
                foreach (var list in ChildLists(node))
                    list.CollectionChanged += ChildrenChanged;
            }
            foreach (var list in ChildLists(node))
                foreach (FlowBase child in list)
                    Watch(child);
        }


        private void Unwatch(FlowBase node)
        {
            if (node == null || !watched.TryGetValue(node, out var count))
                return;
            foreach (var list in ChildLists(node))
                foreach (FlowBase child in list)
                    Unwatch(child);
            if (count > 1)
            {
                watched[node] = count - 1;
                return;
            }
            watched.Remove(node);
            node.PropertyChanged -= NodePropertyChanged;
            foreach (var list in ChildLists(node))
                list.CollectionChanged -= ChildrenChanged;
        }


        private void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e is FlowPropertyChangedEventArgs change && sender is FlowBase node)
                Record(new PropertyChange(node, change.PropertyName, change.OldValue, change.NewValue));
        }


        private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = (IFlowNodeCollection)sender;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var node = (FlowBase)e.NewItems[i];
                        Watch(node);
                        Record(new InsertChange(list, e.NewStartingIndex + i, node));
                        if (!replaying)
                            NodeAdded?.Invoke(node);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var node = (FlowBase)e.OldItems[i];
                        Unwatch(node);
                        Record(new RemoveChange(list, e.OldStartingIndex, node));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldNode = (FlowBase)e.OldItems[0];
                    var newNode = (FlowBase)e.NewItems[0];
                    Watch(newNode);
                    Unwatch(oldNode);
                    Record(new ReplaceChange(list, e.NewStartingIndex, oldNode, newNode));
                    break;
                case NotifyCollectionChangedAction.Move:
                    Record(new MoveChange(list, e.OldStartingIndex, e.NewStartingIndex));
                    break;
                default:
                    // FlowNodeCollection never resets (it clears node by node); a reset could not be undone, so the history is forgotten
                    if (!replaying)
                        Clear();
                    break;
            }
        }


        private void Record(Change change)
        {
            if (replaying || detached)
                return;
            redoSteps.Clear();
            (openStep ??= new List<Change>()).Add(change);
            OnChanged();
        }


        private void Replay(Action action)
        {
            replaying = true;
            try
            {
                action();
            }
            finally
            {
                replaying = false;
            }
        }


        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }


        private abstract class Change
        {
            public abstract void Undo();
            public abstract void Redo();
        }


        private sealed class PropertyChange : Change
        {
            private readonly FlowBase node;
            private readonly string name;
            private readonly object oldValue;
            private readonly object newValue;

            public PropertyChange(FlowBase node, string name, object oldValue, object newValue)
            {
                this.node = node;
                this.name = name;
                this.oldValue = oldValue;
                this.newValue = newValue;
            }

            public FlowBase Node => node;
            public string Name => name;
            public object OldValue => oldValue;
            public object NewValue => newValue;

            public override void Undo() => node.GetType().GetProperty(name)?.SetValue(node, oldValue);
            public override void Redo() => node.GetType().GetProperty(name)?.SetValue(node, newValue);
        }


        private sealed class InsertChange : Change
        {
            private readonly IList list;
            private readonly int index;
            private readonly FlowBase node;

            public InsertChange(IList list, int index, FlowBase node)
            {
                this.list = list;
                this.index = index;
                this.node = node;
            }

            public override void Undo() => list.RemoveAt(index);
            public override void Redo() => list.Insert(index, node);
        }


        private sealed class RemoveChange : Change
        {
            private readonly IList list;
            private readonly int index;
            private readonly FlowBase node;

            public RemoveChange(IList list, int index, FlowBase node)
            {
                this.list = list;
                this.index = index;
                this.node = node;
            }

            public override void Undo() => list.Insert(index, node);
            public override void Redo() => list.RemoveAt(index);
        }


        private sealed class ReplaceChange : Change
        {
            private readonly IList list;
            private readonly int index;
            private readonly FlowBase oldNode;
            private readonly FlowBase newNode;

            public ReplaceChange(IList list, int index, FlowBase oldNode, FlowBase newNode)
            {
                this.list = list;
                this.index = index;
                this.oldNode = oldNode;
                this.newNode = newNode;
            }

            public override void Undo() => list[index] = oldNode;
            public override void Redo() => list[index] = newNode;
        }


        private sealed class MoveChange : Change
        {
            private readonly IFlowNodeCollection list;
            private readonly int oldIndex;
            private readonly int newIndex;

            public MoveChange(IFlowNodeCollection list, int oldIndex, int newIndex)
            {
                this.list = list;
                this.oldIndex = oldIndex;
                this.newIndex = newIndex;
            }

            public override void Undo() => list.Move(newIndex, oldIndex);
            public override void Redo() => list.Move(oldIndex, newIndex);
        }
    }
}
