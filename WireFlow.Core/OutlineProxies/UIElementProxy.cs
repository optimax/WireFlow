using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WireFlow
{
    public class UIElementProxy : INotifyPropertyChanged
    {
        private static readonly ImageSource _imageSource = RetrieveImageBitmap("element.png");

        private bool isHighlighted;


        public event PropertyChangedEventHandler PropertyChanged;

        public FrameworkElement UIElement { get; set; }

        /// <summary>Whether the element is selected in the Designer, which the Outline shows as a highlight.</summary>
        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted == value)
                    return;
                isHighlighted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHighlighted)));
            }
        }

        public string Type { get; set; }
        public string Name { get; set; }

        public double Left => Canvas.GetLeft(UIElement);
        public double Top => Canvas.GetTop(UIElement);
        public double Width => UIElement.Width;
        public double Height => UIElement.Height;

        public virtual ImageSource ImageSource => _imageSource;

        /// <summary>
        /// The name of the tool glyph the Outline draws before the entry (an SVG of the Designer's Glyphs): a rectangle's and an
        /// ellipse's are their tools', and an element of another kind without one of its own has "element".
        /// </summary>
        public virtual string Glyph => UIElement switch
        {
            WireFlowRectangle => "rectangle",
            WireFlowEllipse => "ellipse",
            _ => "element"
        };

        public virtual string ContentInfo => string.IsNullOrWhiteSpace(Name)? $"({AutoStr(Left)},{AutoStr(Top)},{AutoStr(Width)},{AutoStr(Height)})": Name;

        public override string ToString()
        {
            return $"{Type}: {Name}";
        }


        /// <summary>The element's node, whose changes the entry shows as they happen; null for an element without one.</summary>
        public FlowBase Node { get; }


        public UIElementProxy(FrameworkElement element)
        {
            UIElement = element;
            Node = FlowView.GetNode(element);
            Type = KindName(element, Node);
            Name = Node?.Name ?? element.Name;
            if (Node != null)
                Node.PropertyChanged += NodePropertyChanged;
        }


        // What the Outline calls an element that has no entry class of its own, as the tools and Properties name it: a
        // rectangle and an ellipse by their tools' names, not by their controls' classes; an element of another kind by its
        // kind, and one without a node by its class
        private static string KindName(FrameworkElement element, FlowBase node) => element switch
        {
            WireFlowRectangle => "Rectangle",
            WireFlowEllipse => "Ellipse",
            _ => string.IsNullOrWhiteSpace(node?.Type) ? element.GetType().Name : node.Type
        };


        private void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
            // What holds an element hides and locks it too, so the entries under a holder show its eye and padlock as theirs
            if (e.PropertyName == nameof(FlowElementBase.Visible) || e.PropertyName == nameof(FlowElementBase.Locked) || e.PropertyName == nameof(FlowLayer.Hidden))
                RefreshElements();
        }


        /// <summary>Shows the entries under this one as they are now; an entry without any does nothing.</summary>
        protected virtual void RefreshElements()
        {
        }


        #region The eye and the padlock

        /// <summary>
        /// Whether the entry has the Outline's eye and padlock: a layer of a page or layout, or an element on one, the
        /// page's own or those of the layouts it shows, which change their layout. An element of a part has none.
        /// </summary>
        public bool HasState => FlowPath.Of(Node, out _) != null;


        /// <summary>Whether the element or layer is set to show: an element's Visible, a layer's not being Hidden.</summary>
        public bool Visible => Node is FlowElementBase element ? element.Visible : !(Node is FlowLayer layer) || !layer.Hidden;


        /// <summary>Whether the element or layer is itself locked.</summary>
        public bool Locked => Node is FlowElementBase element ? element.Locked : Node is FlowLayer layer && layer.Locked;


        /// <summary>Whether it shows on the page: it is set to show, and so is everything holding it, its layer included.</summary>
        public bool IsShown
        {
            get
            {
                for (var node = Node; node != null; node = node.Parent)
                    if ((node as FlowElementBase)?.Visible == false || (node as FlowLayer)?.Hidden == true)
                        return false;
                return true;
            }
        }


        /// <summary>Whether it is kept from being changed: it is locked, or something holding it is, its layer included.</summary>
        public bool IsLocked
        {
            get
            {
                for (var node = Node; node != null; node = node.Parent)
                    if ((node as FlowElementBase)?.Locked == true || (node as FlowLayer)?.Locked == true)
                        return true;
                return false;
            }
        }


        /// <summary>The glyph of the entry's eye: open while the element shows, struck through while it does not.</summary>
        public string EyeGlyph => IsShown ? "eye" : "eye-off";


        /// <summary>The glyph of the entry's padlock: closed while the element is kept from being changed, open while it is not.</summary>
        public string LockGlyph => IsLocked ? "lock" : "unlock";

        #endregion


        /// <summary>Shows the element's name, place and size as they are now.</summary>
        public void Refresh()
        {
            Name = Node?.Name ?? UIElement.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }


        /// <summary>Stops following the node, as the entry leaves the Outline.</summary>
        public virtual void Detach()
        {
            if (Node != null)
                Node.PropertyChanged -= NodePropertyChanged;
        }


        public string AutoStr(double value)
        {
            if (double.IsNaN(value))
                return "Auto";
            else
                return value.ToString("N0");
        }


        protected static BitmapImage RetrieveImageBitmap(string imageName)
        {
            return ImageUtils.RetrieveImageFromResource("WireFlow.Core",imageName);
        }

    }
}
