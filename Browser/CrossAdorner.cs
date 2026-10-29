using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WireFlow
{
    public class CrossAdorner : Adorner
    {
        public CrossAdorner(UIElement adornedElement) : base(adornedElement)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(new Pen(Brushes.Crimson, 0.5), new Point(0, ActualHeight / 2), new Point(ActualWidth, ActualHeight / 2));
            drawingContext.DrawLine(new Pen(Brushes.Crimson, 0.5), new Point(ActualWidth / 2, 0), new Point(ActualWidth / 2, ActualHeight));
            //drawingContext.DrawRectangle(Brushes.Red, null, new System.Windows.Rect(0, 0, 5, 5));
            //drawingContext.DrawRectangle(Brushes.Red, null, new System.Windows.Rect(0, ActualHeight - 5, 5, 5));
            //drawingContext.DrawRectangle(Brushes.Red, null, new System.Windows.Rect(ActualWidth - 5, 0, 5, 5));
            //drawingContext.DrawRectangle(Brushes.Red, null, new System.Windows.Rect(ActualWidth - 5, ActualHeight - 5, 5, 5));
        }
    }
}
