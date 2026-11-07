using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{
    /// <summary>The ellipse element (kind "ellipse"): an Ellipse as large as the element, whose look it forwards.</summary>
    public class WireFlowEllipse : BaseDesignerObject
    {
        private readonly Ellipse ellipse = new Ellipse();


        public WireFlowEllipse()
        {
            Content = ellipse;
        }


        [Category("Appearance")]
        public Brush Fill
        {
            get => ellipse.Fill;
            set => ellipse.Fill = value;
        }

        [Category("Appearance")]
        public Brush Stroke
        {
            get => ellipse.Stroke;
            set => ellipse.Stroke = value;
        }

        [Category("Appearance")]
        public double StrokeThickness
        {
            get => ellipse.StrokeThickness;
            set => ellipse.StrokeThickness = value;
        }
    }
}
