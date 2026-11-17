using System;
using System.Collections.Generic;
using System.Windows;

namespace WireFlow
{
    /// <summary>
    /// The room an element can fill in its container, apart from any control: how far its edges can go out before they
    /// meet another element or the container's edge.
    /// </summary>
    public static class FillLayout
    {
        // An overlap of less than this counts as touching
        private const double TOLERANCE = 0.5;


        /// <summary>
        /// The element made to fill the room around it, across, down, or both. Across, its left and right edges go out to
        /// the nearest of the others that lie beside it (at its height, wholly to its left or right), else to the
        /// container's edges; down, its top and bottom edges likewise. The others it overlaps do not count: they lie under
        /// or over it. Filling both ways, the larger of across-then-down and down-then-across is taken.
        /// </summary>
        public static Rect Fill(Rect element, Rect container, IReadOnlyList<Rect> others, bool across, bool down)
        {
            if (across && down)
            {
                var acrossFirst = Down(Across(element, container, others), container, others);
                var downFirst = Across(Down(element, container, others), container, others);
                return downFirst.Width * downFirst.Height > acrossFirst.Width * acrossFirst.Height ? downFirst : acrossFirst;
            }
            if (across)
                return Across(element, container, others);
            return down ? Down(element, container, others) : element;
        }


        private static Rect Across(Rect element, Rect container, IReadOnlyList<Rect> others)
        {
            double left = container.Left, right = container.Right;
            foreach (var other in others)
            {
                if (!HasExtent(other) || other.Bottom <= element.Top + TOLERANCE || other.Top >= element.Bottom - TOLERANCE)
                    continue;
                if (other.Right <= element.Left + TOLERANCE)
                    left = Math.Max(left, other.Right);
                else if (other.Left >= element.Right - TOLERANCE)
                    right = Math.Min(right, other.Left);
            }
            return right > left ? new Rect(left, element.Top, right - left, element.Height) : element;
        }


        private static Rect Down(Rect element, Rect container, IReadOnlyList<Rect> others)
        {
            double top = container.Top, bottom = container.Bottom;
            foreach (var other in others)
            {
                if (!HasExtent(other) || other.Right <= element.Left + TOLERANCE || other.Left >= element.Right - TOLERANCE)
                    continue;
                if (other.Bottom <= element.Top + TOLERANCE)
                    top = Math.Max(top, other.Bottom);
                else if (other.Top >= element.Bottom - TOLERANCE)
                    bottom = Math.Min(bottom, other.Top);
            }
            return bottom > top ? new Rect(element.Left, top, element.Width, bottom - top) : element;
        }


        private static bool HasExtent(Rect rect) => !rect.IsEmpty && rect.Width > 0 && rect.Height > 0;
    }
}
