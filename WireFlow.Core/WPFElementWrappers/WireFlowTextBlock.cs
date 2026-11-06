using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WireFlow
{
    /// <summary>
    /// The text element: a TextBlock in a frame of its own size, with its lines aligned across the frame by TextAlignment and
    /// down it by TextVerticalAlignment, which TextBlock itself does not have. TextBlock seals its measure and render, so
    /// the lines are moved down by the top padding: centered or at the bottom, the top padding is the bottom one (the
    /// padding a text is given is even) and the room the lines leave.
    /// </summary>
    public class WireFlowTextBlock : TextBlock
    {
        /// <summary>Where the text lies down its frame: Top (and Stretch), Center or Bottom.</summary>
        public static readonly DependencyProperty TextVerticalAlignmentProperty = DependencyProperty.Register(nameof(TextVerticalAlignment),
            typeof(VerticalAlignment), typeof(WireFlowTextBlock), new FrameworkPropertyMetadata(VerticalAlignment.Top, TextVerticalAlignmentChanged));

        // Measures the lines alone, with the text's own font and wrapping
        private TextBlock measurer;
        private bool aligning;


        public double Left
        {
            get
            {
                return Canvas.GetLeft(this);
            }

            set
            {
                Canvas.SetLeft(this, value);
            }
        }

        public double Top
        {
            get
            {
                return Canvas.GetTop(this);
            }

            set
            {
                Canvas.SetTop(this, value);
            }
        }

        /// <summary>The Designer sets it, with TextAlignment, through the position chooser of the TextAlignment row.</summary>
        [Browsable(false)]
        public VerticalAlignment TextVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(TextVerticalAlignmentProperty);
            set => SetValue(TextVerticalAlignmentProperty, value);
        }


        public WireFlowTextBlock()
        {
            SizeChanged += (sender, e) => AlignVertically();
        }


        private static void TextVerticalAlignmentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var block = (WireFlowTextBlock)sender;
            // Back at the top, the lines lose what centering them added
            if (!IsMoved((VerticalAlignment)e.NewValue) && IsMoved((VerticalAlignment)e.OldValue))
                block.SetTopPadding(block.Padding.Bottom);
            block.AlignVertically();
        }


        // TextBlock seals OnPropertyChanged: the properties that change the lines' height tell the text through their metadata
        static WireFlowTextBlock()
        {
            foreach (var property in new[] { TextProperty, FontSizeProperty, FontFamilyProperty, FontWeightProperty, FontStyleProperty,
                         FontStretchProperty, TextWrappingProperty, LineHeightProperty, LineStackingStrategyProperty, PaddingProperty,
                         WidthProperty, HeightProperty })
                property.OverrideMetadata(typeof(WireFlowTextBlock), new FrameworkPropertyMetadata(LinesChanged));
        }


        private static void LinesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((WireFlowTextBlock)sender).AlignVertically();
        }


        private static bool IsMoved(VerticalAlignment alignment) => alignment == VerticalAlignment.Center || alignment == VerticalAlignment.Bottom;


        // Centered or at the bottom, the top padding is the bottom one and the room the lines leave (all of it, or half)
        private void AlignVertically()
        {
            if (aligning || !IsMoved(TextVerticalAlignment))
                return;
            var padding = Padding;
            double height = double.IsNaN(Height) ? ActualHeight : Height;
            double width = (double.IsNaN(Width) ? ActualWidth : Width) - padding.Left - padding.Right;
            double offset = 0;
            if (height > 0 && width > 0 && !string.IsNullOrEmpty(Text))
            {
                measurer ??= new TextBlock();
                measurer.Text = Text;
                measurer.FontFamily = FontFamily;
                measurer.FontSize = FontSize;
                measurer.FontWeight = FontWeight;
                measurer.FontStyle = FontStyle;
                measurer.FontStretch = FontStretch;
                measurer.TextWrapping = TextWrapping;
                measurer.LineHeight = LineHeight;
                measurer.LineStackingStrategy = LineStackingStrategy;
                measurer.Measure(new Size(width, double.PositiveInfinity));
                double room = height - measurer.DesiredSize.Height - 2 * padding.Bottom;
                if (room > 0)
                    offset = TextVerticalAlignment == VerticalAlignment.Center ? room / 2 : room;
            }
            SetTopPadding(padding.Bottom + offset);
        }


        private void SetTopPadding(double top)
        {
            var padding = Padding;
            if (padding.Top == top)
                return;
            aligning = true;
            try
            {
                Padding = new Thickness(padding.Left, top, padding.Right, padding.Bottom);
            }
            finally
            {
                aligning = false;
            }
        }
    }
}
