using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{
    /// <summary>The rectangle element (kind "rect"): a Rectangle as large as the element, whose look it forwards.</summary>
    public class WireFlowRectangle : BaseDesignerObject
    {
        private readonly Rectangle rectangle = new Rectangle();


        public WireFlowRectangle()
        {
            Content = rectangle;
        }


        [Category("Appearance")]
        public Brush Fill
        {
            get => rectangle.Fill;
            set => rectangle.Fill = value;
        }

        [Category("Appearance")]
        public Brush Stroke
        {
            get => rectangle.Stroke;
            set => rectangle.Stroke = value;
        }

        [Category("Appearance")]
        public double StrokeThickness
        {
            get => rectangle.StrokeThickness;
            set => rectangle.StrokeThickness = value;
        }

        [Category("Appearance")]
        public double RadiusX
        {
            get => rectangle.RadiusX;
            set => rectangle.RadiusX = value;
        }

        [Category("Appearance")]
        public double RadiusY
        {
            get => rectangle.RadiusY;
            set => rectangle.RadiusY = value;
        }
    }
}
