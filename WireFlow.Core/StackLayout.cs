using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WireFlow
{
    /// <summary>
    /// The layout of a stack panel, apart from any control: where each child goes, given the panel's size, its direction
    /// and the children's sizes.
    /// </summary>
    public static class StackLayout
    {
        /// <summary>
        /// The children one after another in the direction: each keeps its size in that direction (none if it has none, as
        /// an automatic size) and gets the panel's size across it. They start at the panel's top left corner, or, stacked
        /// from the end, at its bottom (its right side, in a horizontal panel): the first child lies against that end and
        /// each next one before it.
        /// </summary>
        public static Rect[] Arrange(Size panel, Orientation direction, IReadOnlyList<Size> children, bool fromEnd = false)
        {
            var result = new Rect[children.Count];
            double offset = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (direction == Orientation.Vertical)
                {
                    double height = Length(children[i].Height);
                    result[i] = new Rect(0, fromEnd ? Length(panel.Height) - offset - height : offset, Length(panel.Width), height);
                    offset += height;
                }
                else
                {
                    double width = Length(children[i].Width);
                    result[i] = new Rect(fromEnd ? Length(panel.Width) - offset - width : offset, 0, width, Length(panel.Height));
                    offset += width;
                }
            }
            return result;
        }

        // A size that is not a number or negative counts as none
        private static double Length(double value) => double.IsFinite(value) && value > 0 ? value : 0;
    }
}
