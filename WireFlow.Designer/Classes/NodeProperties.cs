using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WireFlow
{
    /// <summary>
    /// What Properties shows of a DOM node: the properties of its own that its kind of element has (a rectangle's fill and
    /// stroke, a text's font and alignment, ...), under the names the Designer has shown them by, rather than every property
    /// of the WPF control that draws it. The values are the node's, set in the node, which the view follows.
    /// </summary>
    public sealed class NodeProperties : ICustomTypeDescriptor
    {
        // A property shown: the node's property, the name and category it is shown with, and whether it can be changed
        private sealed class Shown
        {
            public string Property, Name, Category;
            public bool ReadOnly;
        }


        /// <summary>Whether another node has an ID; an ID typed in that another has is refused (the row shows why).</summary>
        public static Func<FlowBase, string, bool> IdTaken { get; set; }

        /// <summary>
        /// Renames a page of the project (one with a key) to the name typed in, before its node takes it: gives the reason it
        /// cannot (the name is taken, or empty), or null once it is renamed.
        /// </summary>
        public static Func<FlowPage, string, string> RenamePage { get; set; }

        /// <summary>
        /// Checks a layout typed or chosen for a page or layout (its <see cref="FlowPage.Master"/>, a layout's name or ID): gives
        /// the reason it cannot be set (it names no layout, or makes layouts set on each other in a circle), or null.
        /// </summary>
        public static Func<FlowPage, string, string> CheckLayout { get; set; }

        /// <summary>Tells that a node's ID or a page's name changed (the node, the old and the new), for the links naming it.</summary>
        public static Action<FlowBase, string, string> LinkTargetChanged { get; set; }


        private static Shown P(string property, string category, string name = null, bool readOnly = false) =>
            new Shown { Property = property, Name = name ?? property, Category = category, ReadOnly = readOnly };


        private static readonly Shown[] Position = { P("Left", "Layout"), P("Top", "Layout") };
        private static readonly Shown[] Size = { P("Width", "Layout"), P("Height", "Layout") };
        private static readonly Shown[] Common = { P("Name", "Common"), P("Id", "Common", "ID"), P("Href", "Navigation") };
        private static readonly Shown[] Font =
        {
            P("Color", "Text", "Foreground"), P("Font", "Text", "FontFamily"), P("FontSize", "Text"), P("FontWeight", "Text"),
            P("FontStretch", "Text"), P("FontStyle", "Text"), P("LineHeight", "Text"), P("Text", "Text"), P("TextAlignment", "Text"),
            P("VerticalAlignment", "Text", "TextVerticalAlignment"), P("TextWrapping", "Text"),
        };

        private static readonly Dictionary<string, Shown[]> Kinds = new Dictionary<string, Shown[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["rect"] = Common.Concat(Position).Concat(Size).Concat(new[]
            {
                P("Fill", "Appearance"), P("Transparency", "Appearance"), P("Stroke", "Appearance"), P("StrokeThickness", "Appearance"),
                P("RadiusX", "Appearance"), P("RadiusY", "Appearance"),
            }).ToArray(),
            ["ellipse"] = Common.Concat(Position).Concat(Size).Concat(new[]
            {
                P("Fill", "Appearance"), P("Transparency", "Appearance"), P("Stroke", "Appearance"), P("StrokeThickness", "Appearance"),
            }).ToArray(),
            ["text"] = Common.Concat(Position).Concat(Size).Concat(Font).Concat(new[]
            {
                P("Fill", "Appearance", "Background"), P("Transparency", "Appearance"), P("StrokeThickness", "Text", "TextPadding"),
                P("HorizontalAlignment", "Text", "TextHorizontalAlignment"),
            }).ToArray(),
            ["box"] = Common.Concat(Position).Concat(Size).Concat(Font).Concat(new[]
            {
                P("Fill", "Appearance"), P("Transparency", "Appearance"), P("Stroke", "Appearance", "BorderColor"),
                P("StrokeThickness", "Appearance", "BorderWidth"), P("RadiusX", "Appearance", "BorderRadiusX"),
                P("RadiusY", "Appearance", "BorderRadiusY"), P("HorizontalAlignment", "Text", "TextHorizontalAlignment"),
            }).ToArray(),
            ["image"] = Common.Concat(Position).Concat(Size).Concat(new[] { P("Image", "Image"), P("Stretch", "Image") }).ToArray(),
            ["group"] = Common.Concat(Position).ToArray(),
            ["stack"] = Common.Concat(Position).Concat(Size).Concat(new[]
            {
                P("Direction", "Layout"), P("Fill", "Appearance", "Background"), P("Transparency", "Appearance"),
            }).ToArray(),
            ["part"] = new[] { P("Name", "Common", "Part"), P("Id", "Common", "ID"), P("Href", "Navigation") }.Concat(Position).Concat(Size).ToArray(),
        };

        private static readonly Shown[] Unknown = new[] { P("Name", "Common"), P("Id", "Common", "ID"), P("Type", "Common", readOnly: true) }.Concat(Position).Concat(Size).ToArray();

        private static readonly Shown[] Page =
        {
            P("Name", "Common"), P("Id", "Common", "ID"), P("Master", "Common", "Layout"),
            P("Width", "Layout"), P("Height", "Layout"), P("Background", "Appearance"),
        };


        private readonly WireFlowEngine engine;
        private PropertyDescriptorCollection properties;
        // The nodes shown, when several are: what each of them shows, of which this one shows what they have in common
        private readonly NodeProperties[] parts;


        /// <summary>The node shown; the first, when several are.</summary>
        public FlowBase Node { get; }

        /// <summary>The nodes shown.</summary>
        public IReadOnlyList<FlowBase> Nodes => parts == null ? new[] { Node } : parts.Select(p => p.Node).ToArray();


        public NodeProperties(FlowBase node, WireFlowEngine engine)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            this.engine = engine;
        }


        /// <summary>
        /// Shows the properties that all the nodes show, under the same name and of the same type, but their Name, which each keeps:
        /// a value shows where all the nodes have it, and is empty where they differ; a value set is set on all of them.
        /// </summary>
        public NodeProperties(IReadOnlyList<FlowBase> nodes, WireFlowEngine engine) : this(nodes?.FirstOrDefault(), engine)
        {
            if (nodes.Count > 1)
                parts = nodes.Select(n => new NodeProperties(n, engine)).ToArray();
        }


        public override string ToString() => parts == null ? Node.Name : $"{parts.Length} objects";


        private PropertyDescriptorCollection Properties => properties ?? (properties = parts == null ? MakeProperties() : MakeCommonProperties());


        private PropertyDescriptorCollection MakeCommonProperties()
        {
            var list = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor first in parts[0].Properties)
            {
                // Each keeps its own name and ID
                if (first.Name == "Name" || first.Name == "ID")
                    continue;
                var each = parts.Select(p => p.Properties[first.Name]).ToArray();
                if (each.Any(d => d == null || d.PropertyType != first.PropertyType))
                    continue;
                list.Add(new CommonPropertyDescriptor(first, parts, each));
            }
            return new PropertyDescriptorCollection(list.ToArray(), true);
        }


        private PropertyDescriptorCollection MakeProperties()
        {
            var shown = Node is FlowPage ? Page : Kinds.TryGetValue(Node.Type ?? "", out var kind) ? kind : Unknown;
            var own = TypeDescriptor.GetProperties(Node);
            var list = new List<PropertyDescriptor>();
            foreach (var p in shown)
            {
                var inner = own[p.Property];
                if (inner == null)
                    continue;
                Func<FlowBase, object> get = null;
                Action<FlowBase, object> set = null;
                if (Node is FlowPage && (p.Property == "Width" || p.Property == "Height"))
                {
                    // The size it is shown at, its own or inherited; one set is its own, unless it has none and it is the inherited one
                    bool width = p.Property == "Width";
                    get = n => (double)inner.GetValue(n) is double own2 && !double.IsNaN(own2) ? own2 : Inherited((FlowPage)n, width);
                    set = (n, v) =>
                    {
                        var page = (FlowPage)n;
                        double value = (double)v;
                        bool hasOwn = !double.IsNaN((double)inner.GetValue(n));
                        inner.SetValue(n, !hasOwn && value == Inherited(page, width) ? double.NaN : value);
                    };
                }
                else if ((Node.Type == "box" || Node.Type == "text") && p.Property == "HorizontalAlignment")
                    // Where a box's or text's text lies across it carries where its lines lie with it, but for Stretch
                    set = (n, v) =>
                    {
                        inner.SetValue(n, v);
                        var element = (FlowElement)n;
                        switch ((FlowHorizontalAlignment)v)
                        {
                            case FlowHorizontalAlignment.Center:
                                element.TextAlignment = FlowTextAlignment.Center;
                                break;
                            case FlowHorizontalAlignment.Right:
                                element.TextAlignment = FlowTextAlignment.Right;
                                break;
                            case FlowHorizontalAlignment.Left:
                                element.TextAlignment = FlowTextAlignment.Left;
                                break;
                        }
                    };
                list.Add(new NodePropertyDescriptor(p.Name, p.Category, inner, p.ReadOnly, get, set));
            }
            return new PropertyDescriptorCollection(list.ToArray(), true);
        }


        private double Inherited(FlowPage page, bool width)
        {
            var size = engine.InheritedPageSize(page);
            return width ? size.Width : size.Height;
        }


        #region ICustomTypeDescriptor

        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => parts != null ? $"{parts.Length} objects" : Node is FlowPage page ? (page.IsMaster ? "Layout" : "Page") : KindName(Node.Type);


        // The name a kind of element is shown by (Properties' header), as the tools and the Outline name it
        private static string KindName(string kind)
        {
            switch (kind)
            {
                case "rect": return "Rectangle";
                case "ellipse": return "Ellipse";
                case "text": return "Text";
                case "box": return "Box";
                case "image": return "Image";
                case "group": return "Group";
                case "stack": return "Stack Panel";
                case "part": return "Part";
                default: return kind;
            }
        }
        public string GetComponentName() => parts == null ? Node.Name : null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public PropertyDescriptorCollection GetProperties() => Properties;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => Properties;
        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        #endregion



        /// <summary>
        /// A property that several nodes show: its value is theirs where they agree, else null; a value set is set on each of them,
        /// through what each shows (so a page's inherited size, or an alignment carrying another, works as for one node). It tells
        /// of a change of the property of any of them.
        /// </summary>
        private sealed class CommonPropertyDescriptor : PropertyDescriptor
        {
            private readonly NodeProperties[] parts;
            private readonly PropertyDescriptor[] each;
            private readonly Dictionary<object, EventHandler> watches = new Dictionary<object, EventHandler>();


            public CommonPropertyDescriptor(PropertyDescriptor first, NodeProperties[] parts, PropertyDescriptor[] each)
                : base(first.Name, new Attribute[] { new CategoryAttribute(first.Category) })
            {
                this.parts = parts;
                this.each = each;
            }


            public override Type ComponentType => typeof(NodeProperties);
            public override Type PropertyType => each[0].PropertyType;
            public override bool IsReadOnly => each.Any(d => d.IsReadOnly);
            public override TypeConverter Converter => each[0].Converter;
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;


            public override object GetValue(object component)
            {
                var value = each[0].GetValue(parts[0]);
                for (int i = 1; i < parts.Length; i++)
                    if (!Equals(each[i].GetValue(parts[i]), value))
                        return null;
                return value;
            }


            public override void SetValue(object component, object value)
            {
                for (int i = 0; i < parts.Length; i++)
                    each[i].SetValue(parts[i], value);
            }


            public override void AddValueChanged(object component, EventHandler handler)
            {
                base.AddValueChanged(component, handler);
                if (watches.ContainsKey(component))
                    return;
                EventHandler watch = (sender, e) => OnValueChanged(component, EventArgs.Empty);
                for (int i = 0; i < parts.Length; i++)
                    each[i].AddValueChanged(parts[i], watch);
                watches[component] = watch;
            }


            public override void RemoveValueChanged(object component, EventHandler handler)
            {
                base.RemoveValueChanged(component, handler);
                if (GetValueChangedHandler(component) != null || !watches.TryGetValue(component, out var watch))
                    return;
                for (int i = 0; i < parts.Length; i++)
                    each[i].RemoveValueChanged(parts[i], watch);
                watches.Remove(component);
            }
        }



        /// <summary>
        /// A property of the node, shown under its own name and category; it tells of a change of the node's property made
        /// anywhere (the node announces each), so the grid's row follows the element as it is moved or resized.
        /// </summary>
        private sealed class NodePropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyDescriptor inner;
            private readonly bool readOnly;
            private readonly Func<FlowBase, object> get;
            private readonly Action<FlowBase, object> set;
            private readonly Dictionary<object, PropertyChangedEventHandler> watches = new Dictionary<object, PropertyChangedEventHandler>();


            public NodePropertyDescriptor(string name, string category, PropertyDescriptor inner, bool readOnly,
                Func<FlowBase, object> get, Action<FlowBase, object> set)
                : base(name, new Attribute[] { new CategoryAttribute(category) })
            {
                this.inner = inner;
                this.readOnly = readOnly;
                this.get = get;
                this.set = set;
            }


            private static FlowBase NodeOf(object component) => ((NodeProperties)component).Node;

            public override Type ComponentType => typeof(NodeProperties);
            public override Type PropertyType => inner.PropertyType;
            public override bool IsReadOnly => readOnly || inner.IsReadOnly;
            public override TypeConverter Converter => inner.Converter;
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;


            public override object GetValue(object component) => get != null ? get(NodeOf(component)) : inner.GetValue(NodeOf(component));


            public override void SetValue(object component, object value)
            {
                var node = NodeOf(component);
                if (inner.Name == nameof(FlowBase.Id) && value is string id && IdTaken?.Invoke(node, id.Trim()) == true)
                    throw new ArgumentException($"The ID \"{id.Trim()}\" is already used in the project.");
                bool isTarget = inner.Name == nameof(FlowBase.Id) || (inner.Name == nameof(FlowBase.Name) && node is FlowPage);
                string before = isTarget ? inner.GetValue(node) as string : null;
                if (inner.Name == nameof(FlowPage.Master) && node is FlowPage onLayout)
                {
                    // None is null; a layout named is kept as typed, by name or by ID
                    string master = (value as string)?.Trim();
                    value = string.IsNullOrEmpty(master) ? null : master;
                    string refusal = value == null ? null : CheckLayout?.Invoke(onLayout, master);
                    if (refusal != null)
                        throw new ArgumentException(refusal);
                }
                if (inner.Name == nameof(FlowBase.Name) && node is FlowPage page && !string.IsNullOrEmpty(page.Key))
                {
                    // A page's name is also what the project lists it by: it is renamed there first, or not at all
                    string name = (value as string)?.Trim();
                    if (string.Equals(name, page.Name, StringComparison.Ordinal))
                        return;
                    string refusal = RenamePage?.Invoke(page, name);
                    if (refusal != null)
                        throw new ArgumentException(refusal);
                    value = name;
                }
                if (set != null)
                    set(node, value);
                else
                    inner.SetValue(node, value);
                string after = isTarget ? inner.GetValue(node) as string : null;
                if (isTarget && !string.IsNullOrWhiteSpace(before) && !string.IsNullOrWhiteSpace(after) && !string.Equals(before, after, StringComparison.Ordinal))
                    LinkTargetChanged?.Invoke(node, before, after);
            }


            public override void AddValueChanged(object component, EventHandler handler)
            {
                base.AddValueChanged(component, handler);
                if (watches.ContainsKey(component))
                    return;
                PropertyChangedEventHandler watch = (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == inner.Name)
                        OnValueChanged(component, EventArgs.Empty);
                };
                NodeOf(component).PropertyChanged += watch;
                watches[component] = watch;
            }


            public override void RemoveValueChanged(object component, EventHandler handler)
            {
                base.RemoveValueChanged(component, handler);
                if (GetValueChangedHandler(component) != null || !watches.TryGetValue(component, out var watch))
                    return;
                NodeOf(component).PropertyChanged -= watch;
                watches.Remove(component);
            }
        }
    }
}
