using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// A panel on a page (kind "panel"): it places its children by their Alignment, as a Delphi panel does (see
    /// PanelLayout). It sets the position and size of each child it places, so the children keep true positions and
    /// sizes, which the Designer uses as for any element; a child without an alignment lies where its own position puts
    /// it, as on a canvas. The panel clips its children to itself.
    /// </summary>
    public class AlignPanel : BaseElement, ILinkable
    {
        /// <summary>Where the panel holding an element places it; any element can have one, and a panel heeds it.</summary>
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.RegisterAttached("Alignment",
            typeof(FlowAlignment), typeof(AlignPanel), new FrameworkPropertyMetadata(FlowAlignment.None, AlignmentChanged));

        public static FlowAlignment GetAlignment(DependencyObject element) => (FlowAlignment)element.GetValue(AlignmentProperty);

        public static void SetAlignment(DependencyObject element, FlowAlignment value) => element.SetValue(AlignmentProperty, value);


        [System.ComponentModel.Category("Navigation")]
        public string Href { get; set; }


        public AlignPanel()
        {
            ClipToBounds = true;
        }


        // A child aligned another way is placed again
        private static void AlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Visual visual && VisualTreeHelper.GetParent(visual) is AlignPanel panel)
            {
                panel.InvalidateMeasure();
                panel.InvalidateArrange();
            }
        }


        // A child whose size changes changes the panel's measure; its arrangement follows from it
        protected override Size MeasureOverride(Size constraint)
        {
            var size = base.MeasureOverride(constraint);
            InvalidateArrange();
            return size;
        }


        // Each child goes into its place at its new size, as in a stack panel (see Stack.ArrangeOverride): the canvas would
        // arrange it at the size it was measured at, before its size was set here
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var children = InternalChildren.OfType<FrameworkElement>().ToList();
            var panel = new Size(double.IsNaN(Width) ? arrangeSize.Width : Width, double.IsNaN(Height) ? arrangeSize.Height : Height);
            var alignments = children.Select(GetAlignment).ToList();
            var places = PanelLayout.Arrange(panel, alignments, children.Select(c => new Rect(Coordinate(GetLeft(c)), Coordinate(GetTop(c)), c.Width, c.Height)).ToList());
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var place = places[i];
                switch (alignments[i])
                {
                    case FlowAlignment.None:
                        break;
                    case FlowAlignment.Top:
                    case FlowAlignment.Bottom:
                        SetLeft(child, place.X);
                        SetTop(child, place.Y);
                        child.Width = place.Width;
                        break;
                    case FlowAlignment.Left:
                    case FlowAlignment.Right:
                        SetLeft(child, place.X);
                        SetTop(child, place.Y);
                        child.Height = place.Height;
                        break;
                    default:
                        SetLeft(child, place.X);
                        SetTop(child, place.Y);
                        child.Width = place.Width;
                        child.Height = place.Height;
                        break;
                }
                // Measured again first, as its size may have changed; a size left automatic takes the size it asks for
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var size = new Size(double.IsNaN(child.Width) ? child.DesiredSize.Width : child.Width, double.IsNaN(child.Height) ? child.DesiredSize.Height : child.Height);
                child.Arrange(new Rect(new Point(place.X, place.Y), size));
            }
            // Anything else on the panel, as a canvas places it
            foreach (UIElement other in InternalChildren)
                if (!(other is FrameworkElement))
                    other.Arrange(new Rect(new Point(Coordinate(GetLeft(other)), Coordinate(GetTop(other))), other.DesiredSize));
            return arrangeSize;
        }


        private static double Coordinate(double value) => double.IsNaN(value) ? 0 : value;
    }
}
