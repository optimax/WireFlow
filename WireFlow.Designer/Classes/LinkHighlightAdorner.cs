using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// The overlay View > Highlight Links lays over an element that is a link: its whole extent in the application's
    /// <c>LinkHighlightBrush</c>, a color partly transparent (Tools > Options). Clicks go through it to the element.
    /// </summary>
    internal sealed class LinkHighlightAdorner : Adorner
    {
        public LinkHighlightAdorner(UIElement element) : base(element)
        {
            IsHitTestVisible = false;
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (TryFindResource("LinkHighlightBrush") is Brush brush)
                drawingContext.DrawRectangle(brush, null, new Rect(AdornedElement.RenderSize));
        }
    }
}
