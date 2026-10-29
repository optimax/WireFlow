using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{

    /// <summary>
    /// A combination of Rectangle and TextBlock in a Grid.
    /// </summary>
    public class Box : BaseDesignerObject
    {
        private Grid grid;
        private Rectangle rectangle;
        private TextBlock textBlock;

        #region Properties

        [Category("Content")]
        public string Text {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }


        [Category("Appearance")]
        public TextAlignment TextAlignment
        {
            get { return textBlock.TextAlignment; }
            set { textBlock.TextAlignment = value; }
        }

        [Category("Appearance")]
        public TextWrapping TextWrapping
        {
            get { return textBlock.TextWrapping; }
            set { textBlock.TextWrapping = value; }
        }

        [Category("Appearance")]
        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return textBlock.HorizontalAlignment; }
            set { textBlock.HorizontalAlignment = value; }
        }

        [Category("Appearance")]
        public VerticalAlignment TextVerticalAlignment
        {
            get { return textBlock.VerticalAlignment; }
            set { textBlock.VerticalAlignment = value; }
        }

        [Category("Appearance")]
        public Thickness TextPadding
        {
            get { return textBlock.Padding; }
            set { textBlock.Padding = value; }
        }

        [Category("Appearance")]
        public Brush Fill
        {
            get { return rectangle.Fill; }
            set { rectangle.Fill = value; }
        }

        [Category("Appearance")]
        public Brush BorderColor
        {
            get { return rectangle.Stroke; }
            set { rectangle.Stroke = value; }
        }

        [Category("Appearance")]
        public double BorderWidth
        {
            get { return rectangle.StrokeThickness; }
            set { rectangle.StrokeThickness = value; }
        }

        [Category("Appearance")]
        public double BorderRadiusX
        {
            get { return rectangle.RadiusX; }
            set { rectangle.RadiusX = value; }
        }

        [Category("Appearance")]
        public double BorderRadiusY
        {
            get { return rectangle.RadiusY; }
            set { rectangle.RadiusY = value; }
        }


        [Category("Appearance")]
        public double LineHeight
        {
            get { return textBlock.LineHeight; }
            set { textBlock.LineHeight = value; }
        }


        [Category("Appearance")]
        public LineStackingStrategy LineStackingStrategy
        {
            get { return textBlock.LineStackingStrategy; }
            set { textBlock.LineStackingStrategy = value; }
        }


        #endregion


        public Box()
        {
            FontSize = 15;

            grid = new Grid();
            Content = grid;

            rectangle = new Rectangle()
            {
                Fill = Brushes.LightGray,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            grid.Children.Add(rectangle);

            textBlock = new TextBlock()
            {
                Text = "Text in a Box",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
            };
            grid.Children.Add(textBlock);

        }


    }
}
