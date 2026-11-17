using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    /// <summary>
    /// The Pick Color tool: a click takes the colors of the element clicked, its fill and its stroke, and the next click
    /// gives them to the element clicked then; with Shift held, to each element clicked after it too. It holds the colors
    /// taken between the clicks, so that they can be given on another page open.
    /// </summary>
    public class PickColorDrawingTool : ModeSelectionTool
    {
        public override string Title => "Pick Color";
        public override string Glyph => "pick-color";
        public override MouseDragMode DesignerMode => MouseDragMode.Pick;

        /// <summary>An eyedropper to take colors with; a paint bucket once it has some to give.</summary>
        public override Cursor Cursor => HasColors ? Named("PaintBucketCursor", Cursors.Pen) : Named("PickColorCursor", Cursors.Cross);


        // A cursor of the application's resources (Xaml\Cursors.xaml); where the application has none of that name, as a
        // program that hosts the window without them, one of Windows' own
        private static Cursor Named(string key, Cursor fallback) => (Application.Current?.TryFindResource(key) as FrameworkElement)?.Cursor ?? fallback;


        /// <summary>Whether colors were taken and wait to be given.</summary>
        public bool HasColors { get; private set; }

        /// <summary>The fill taken, as the element had it; it has one when the element taken from shows a fill.</summary>
        public string Fill { get; private set; }
        public bool HasFill { get; private set; }

        /// <summary>The stroke taken; it has one when the element taken from shows a stroke.</summary>
        public string Stroke { get; private set; }
        public bool HasStroke { get; private set; }


        /// <summary>Whether an element has a color the tool takes or gives: a fill or a stroke that Properties shows for its kind.</summary>
        public static bool HasColor(FlowBase node) => NodeProperties.Shows(node, nameof(FlowElementBase.Fill)) || ShowsStroke(node);


        private static bool ShowsStroke(FlowBase node) => node is FlowElement && NodeProperties.Shows(node, nameof(FlowElement.Stroke));


        /// <summary>Takes an element's fill and stroke, those of the two its kind has.</summary>
        public void Take(FlowElementBase node)
        {
            HasFill = NodeProperties.Shows(node, nameof(FlowElementBase.Fill));
            Fill = HasFill ? node.Fill : null;
            HasStroke = ShowsStroke(node);
            Stroke = HasStroke ? ((FlowElement)node).Stroke : null;
            HasColors = HasFill || HasStroke;
        }


        /// <summary>
        /// Takes a color that is no element's, as one chosen on a color chart, to give as a fill: the next click gives it to
        /// the element clicked, whose stroke it leaves.
        /// </summary>
        public void Load(string fill)
        {
            HasFill = true;
            Fill = fill;
            HasStroke = false;
            Stroke = null;
            HasColors = true;
        }


        /// <summary>
        /// Gives an element the colors taken: the fill, when one was taken and its kind has a fill, and likewise the stroke.
        /// Says whether it changed.
        /// </summary>
        public bool GiveTo(FlowElementBase node)
        {
            bool changed = false;
            if (HasFill && NodeProperties.Shows(node, nameof(FlowElementBase.Fill)) && node.Fill != Fill)
            {
                node.Fill = Fill;
                changed = true;
            }
            if (HasStroke && ShowsStroke(node) && ((FlowElement)node).Stroke != Stroke)
            {
                ((FlowElement)node).Stroke = Stroke;
                changed = true;
            }
            return changed;
        }


        /// <summary>Forgets the colors taken: the next click takes colors again.</summary>
        public void Clear()
        {
            HasColors = HasFill = HasStroke = false;
            Fill = Stroke = null;
        }
    }
}
