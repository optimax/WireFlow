using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WireFlow
{
    /// <summary>
    /// A stack panel on a page: it places its children one after another in its Direction and stretches them across
    /// itself, as WPF's StackPanel does (see StackLayout). It sets each child's position and its size across the panel,
    /// so the children keep true positions and sizes, which the Designer and the saved page use as for any element.
    /// </summary>
    public class Stack : BaseElement, ILinkable
    {
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction),
            typeof(Orientation), typeof(Stack), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>The direction in which the children follow one another.</summary>
        public Orientation Direction
        {
            get => (Orientation)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }


        public static readonly DependencyProperty StackingFromProperty = DependencyProperty.Register(nameof(StackingFrom),
            typeof(FlowStackingFrom), typeof(Stack), new FrameworkPropertyMetadata(FlowStackingFrom.Beginning, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>The end the children are stacked from: the panel's top or left, or its bottom or right.</summary>
        public FlowStackingFrom StackingFrom
        {
            get => (FlowStackingFrom)GetValue(StackingFromProperty);
            set => SetValue(StackingFromProperty, value);
        }


        [System.ComponentModel.Category("Navigation")]
        public string Href { get; set; }


        public Stack()
        {
            ClipToBounds = true;
        }


        // A child whose size changes changes the panel's measure; its arrangement follows from it
        protected override Size MeasureOverride(Size constraint)
        {
            var size = base.MeasureOverride(constraint);
            InvalidateArrange();
            return size;
        }


        // Each child goes into its place at its new size. The canvas would arrange it at the size it was measured at, before
        // its size across was set here: a child arranged narrower than its width keeps a layout clip of the old size, and
        // shows only that much of itself until something lays it out again.
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            // The children that show: one that is not visible has no place in the stack
            var children = InternalChildren.OfType<FrameworkElement>().Where(c => c.Visibility == Visibility.Visible).ToList();
            var panel = new Size(double.IsNaN(Width) ? arrangeSize.Width : Width, double.IsNaN(Height) ? arrangeSize.Height : Height);
            // A child's own size is its node's: the size its control has across the panel is the one the panel gave it, which,
            // after a change of direction, would be the size the other direction gave
            var own = children.Select(c => FlowView.GetNode(c) is FlowElementBase node ? new Size(NotNegative(node.Width), NotNegative(node.Height)) : new Size(c.Width, c.Height)).ToList();
            var places = StackLayout.Arrange(panel, Direction, own, StackingFrom == FlowStackingFrom.Ending);
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                SetLeft(child, places[i].X);
                SetTop(child, places[i].Y);
                if (Direction == Orientation.Vertical)
                {
                    child.Width = places[i].Width;
                    child.Height = own[i].Height;
                }
                else
                {
                    child.Height = places[i].Height;
                    child.Width = own[i].Width;
                }
                // Measured again first, as its size changed; a size left automatic takes the size it asks for
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var size = new Size(double.IsNaN(child.Width) ? child.DesiredSize.Width : child.Width, double.IsNaN(child.Height) ? child.DesiredSize.Height : child.Height);
                child.Arrange(new Rect(new Point(places[i].X, places[i].Y), size));
            }
            // Anything else on the panel, as a canvas places it
            foreach (UIElement other in InternalChildren)
                if (!(other is FrameworkElement))
                    other.Arrange(new Rect(new Point(Coordinate(GetLeft(other)), Coordinate(GetTop(other))), other.DesiredSize));
            return arrangeSize;
        }


        private static double Coordinate(double value) => double.IsNaN(value) ? 0 : value;


        // A size, which may be automatic (not a number) but not negative
        private static double NotNegative(double value) => value < 0 ? 0 : value;
    }
}
