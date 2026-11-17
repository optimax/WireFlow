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


        private void NodePropertyChanged(object sender, PropertyChangedEventArgs e) => Refresh();


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
