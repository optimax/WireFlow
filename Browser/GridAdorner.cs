using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WireFlow
{
    public class GridAdorner : Adorner
    {
        public GridAdorner(UIElement adornedElement) : base(adornedElement)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double GRID_SIZE = 20;

            Brush darkBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.5 };
            Brush lightBrush = new SolidColorBrush(Colors.LightGray) { Opacity = 0.5 };

            Brush brush;
            double thickness;
            double excess;

            //Vertical
            for (double X = 0; X < ActualWidth + 1; X += GRID_SIZE)
            {
                if (Math.Abs(X % 100) < Constants.EPSILON)
                {
                    brush = lightBrush;
                    thickness = 1.5;
                    excess = 12;
                }
                else
                {
                    brush = darkBrush;
                    thickness = 0.5;
                    excess = 6;
                }
                drawingContext.DrawLine(new Pen(brush, thickness),
                    new Point(X, -excess),
                    new Point(X, ActualHeight + excess));
            }

            // Horizontal
            for (double Y = 0; Y < ActualHeight + 1; Y += GRID_SIZE)
            {
                if (Math.Abs(Y % 100) < Constants.EPSILON)
                {
                    brush = Brushes.Gray;
                    thickness = 1.5;
                    excess = 12;
                }
                else
                {
                    brush = Brushes.LightGray;
                    thickness = 0.5;
                    excess = 6;
                }
                drawingContext.DrawLine(new Pen(brush, thickness),
                    new Point(-excess, Y),
                    new Point(ActualWidth + excess, Y));

            }
        }
    }
}
