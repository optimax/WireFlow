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


        private static Shown P(string property, string category, string name = null, bool readOnly = false) =>
            new Shown { Property = property, Name = name ?? property, Category = category, ReadOnly = readOnly };


        private static readonly Shown[] Position = { P("Left", "Layout"), P("Top", "Layout") };
        private static readonly Shown[] Size = { P("Width", "Layout"), P("Height", "Layout") };
        private static readonly Shown[] Common = { P("Name", "Common"), P("Href", "Navigation") };
        private static readonly Shown[] Font =
        {
            P("Color", "Text", "Foreground"), P("Font", "Text", "FontFamily"), P("FontSize", "Text"), P("FontWeight", "Text"),
            P("FontStretch", "Text"), P("LineHeight", "Text"), P("Text", "Text"), P("TextAlignment", "Text"),
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
            ["part"] = new[] { P("Name", "Common", "Part"), P("Href", "Navigation") }.Concat(Position).Concat(Size).ToArray(),
        };

        private static readonly Shown[] Unknown = new[] { P("Name", "Common"), P("Type", "Common", readOnly: true) }.Concat(Position).Concat(Size).ToArray();

        private static readonly Shown[] Page =
        {
            P("Name", "Common", readOnly: true), P("Master", "Common", "Layout", readOnly: true),
            P("Width", "Layout"), P("Height", "Layout"), P("Background", "Appearance"),
        };


        private readonly WireFlowEngine engine;
        private PropertyDescriptorCollection properties;


        /// <summary>The node shown.</summary>
        public FlowBase Node { get; }


        public NodeProperties(FlowBase node, WireFlowEngine engine)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            this.engine = engine;
        }


        public override string ToString() => Node.Name;


        private PropertyDescriptorCollection Properties => properties ?? (properties = MakeProperties());


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
                else if (Node.Type == "box" && p.Property == "HorizontalAlignment")
                    // Where a box's text lies across it carries where its lines lie with it, but for Stretch
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
        public string GetClassName() => Node is FlowPage page ? (page.IsMaster ? "Layout" : "Page") : Node.Type;
        public string GetComponentName() => Node.Name;
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
                if (set != null)
                    set(NodeOf(component), value);
                else
                    inner.SetValue(NodeOf(component), value);
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
