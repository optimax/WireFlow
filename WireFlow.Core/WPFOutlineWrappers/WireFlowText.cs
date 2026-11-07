using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// The text element (kind "text"): a WireFlowTextBlock as large as the element, which places its lines across and down
    /// the frame, and whose text it forwards. The font and Foreground are the element's own, which the block inherits; the
    /// Background is the element's too, drawn behind the block, and never none (null is Transparent), so that the whole
    /// frame can be clicked.
    /// </summary>
    public class WireFlowText : BaseDesignerObject
    {
        private readonly WireFlowTextBlock block = new WireFlowTextBlock();


        static WireFlowText()
        {
            BackgroundProperty.OverrideMetadata(typeof(WireFlowText),
                new FrameworkPropertyMetadata(Brushes.Transparent, null, (sender, value) => value ?? Brushes.Transparent));
        }


        public WireFlowText()
        {
            Content = block;
        }


        [Category("Content")]
        public string Text
        {
            get => block.Text;
            set => block.Text = value;
        }

        [Category("Appearance")]
        public TextAlignment TextAlignment
        {
            get => block.TextAlignment;
            set => block.TextAlignment = value;
        }

        /// <summary>Where the lines lie down the frame (Stretch is Top).</summary>
        [Category("Appearance")]
        public VerticalAlignment TextVerticalAlignment
        {
            get => block.TextVerticalAlignment;
            set => block.TextVerticalAlignment = value;
        }

        [Category("Appearance")]
        public TextWrapping TextWrapping
        {
            get => block.TextWrapping;
            set => block.TextWrapping = value;
        }

        [Category("Appearance")]
        public Thickness TextPadding
        {
            get => block.Padding;
            set => block.Padding = value;
        }

        [Category("Appearance")]
        public double LineHeight
        {
            get => block.LineHeight;
            set => block.LineHeight = value;
        }

        [Category("Appearance")]
        public LineStackingStrategy LineStackingStrategy
        {
            get => block.LineStackingStrategy;
            set => block.LineStackingStrategy = value;
        }
    }
}
