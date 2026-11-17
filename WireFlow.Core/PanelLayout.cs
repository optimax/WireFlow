using System;
using System.Collections.Generic;
using System.Windows;

namespace WireFlow
{
    /// <summary>
    /// The layout of a panel, apart from any control: where each child goes, given the panel's size and the children's
    /// alignments and own places.
    /// </summary>
    public static class PanelLayout
    {
        /// <summary>
        /// The children's places in the panel, by their alignments, as a Delphi panel places them. The room starts as the
        /// whole panel. The children aligned Top take it first, in their order: each lies at the top of the room left,
        /// across all of it, with its own height, and the room shrinks by it. Then those aligned Bottom, likewise from the
        /// bottom; then Left and then Right, each down all of the room left, with its own width. Those aligned Fill each
        /// take the room that is left after all of these. A child without an alignment keeps its own place.
        /// </summary>
        public static Rect[] Arrange(Size panel, IReadOnlyList<FlowAlignment> alignments, IReadOnlyList<Rect> children)
        {
            var result = new Rect[children.Count];
            double left = 0, top = 0, right = Length(panel.Width), bottom = Length(panel.Height);
            foreach (var side in new[] { FlowAlignment.Top, FlowAlignment.Bottom, FlowAlignment.Left, FlowAlignment.Right })
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (alignments[i] != side)
                        continue;
                    double width = Math.Max(0, right - left), height = Math.Max(0, bottom - top);
                    double own = Length(side == FlowAlignment.Top || side == FlowAlignment.Bottom ? children[i].Height : children[i].Width);
                    switch (side)
                    {
                        case FlowAlignment.Top:
                            result[i] = new Rect(left, top, width, own);
                            top += own;
                            break;
                        case FlowAlignment.Bottom:
                            result[i] = new Rect(left, bottom - own, width, own);
                            bottom -= own;
                            break;
                        case FlowAlignment.Left:
                            result[i] = new Rect(left, top, own, height);
                            left += own;
                            break;
                        default:
                            result[i] = new Rect(right - own, top, own, height);
                            right -= own;
                            break;
                    }
                }
            }
            for (int i = 0; i < children.Count; i++)
            {
                if (alignments[i] == FlowAlignment.Fill)
                    result[i] = new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
                else if (alignments[i] == FlowAlignment.None)
                    result[i] = children[i];
            }
            return result;
        }


        // A size that is not a number or negative counts as none
        private static double Length(double value) => double.IsFinite(value) && value > 0 ? value : 0;
    }
}
