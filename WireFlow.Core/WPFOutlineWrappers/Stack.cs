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
            var children = InternalChildren.OfType<FrameworkElement>().ToList();
            var panel = new Size(double.IsNaN(Width) ? arrangeSize.Width : Width, double.IsNaN(Height) ? arrangeSize.Height : Height);
            var places = StackLayout.Arrange(panel, Direction, children.Select(c => new Size(c.Width, c.Height)).ToList());
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                SetLeft(child, places[i].X);
                SetTop(child, places[i].Y);
                if (Direction == Orientation.Vertical)
                    child.Width = places[i].Width;
                else
                    child.Height = places[i].Height;
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
    }
}
