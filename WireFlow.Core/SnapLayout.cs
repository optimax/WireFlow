using System;
using System.Collections.Generic;
using System.Windows;

namespace WireFlow
{
    /// <summary>
    /// Snapping, apart from any control: the lines an element's edges snap to as it is moved or resized with the mouse (the
    /// edges of the elements around it and of its container), and the nearest of them to an edge.
    /// </summary>
    public static class SnapLayout
    {
        /// <summary>The lines across the container that left and right edges snap to: its own sides and those of the others.</summary>
        public static List<double> LinesAcross(Rect container, IReadOnlyList<Rect> others)
        {
            var lines = new List<double> { container.Left, container.Right };
            foreach (var other in others)
                if (HasExtent(other))
                {
                    lines.Add(other.Left);
                    lines.Add(other.Right);
                }
            return lines;
        }


        /// <summary>The lines down the container that top and bottom edges snap to: its own top and bottom and those of the others.</summary>
        public static List<double> LinesDown(Rect container, IReadOnlyList<Rect> others)
        {
            var lines = new List<double> { container.Top, container.Bottom };
            foreach (var other in others)
                if (HasExtent(other))
                {
                    lines.Add(other.Top);
                    lines.Add(other.Bottom);
                }
            return lines;
        }


        /// <summary>An edge at <paramref name="value"/>, put on the nearest line within the distance; left where it is when none is.</summary>
        public static double Snap(double value, IReadOnlyList<double> lines, double distance)
        {
            double best = value, nearest = distance;
            foreach (double line in lines)
            {
                double away = Math.Abs(line - value);
                if (away <= nearest)
                {
                    nearest = away;
                    best = line;
                }
            }
            return best;
        }


        /// <summary>
        /// How far to move a span, from its start over its length, so that its start or its end lies on a line, whichever
        /// is nearer one within the distance; 0, and no line, when neither is.
        /// </summary>
        public static double SnapSpan(double start, double length, IReadOnlyList<double> lines, double distance, out double? line)
        {
            double atStart = Snap(start, lines, distance), atEnd = Snap(start + length, lines, distance);
            double byStart = atStart - start, byEnd = atEnd - (start + length);
            bool startSnaps = atStart != start || Contains(lines, start), endSnaps = atEnd != start + length || Contains(lines, start + length);
            line = null;
            if (!startSnaps && !endSnaps)
                return 0;
            if (startSnaps && (!endSnaps || Math.Abs(byStart) <= Math.Abs(byEnd)))
            {
                line = atStart;
                return byStart;
            }
            line = atEnd;
            return byEnd;
        }


        private static bool Contains(IReadOnlyList<double> lines, double value)
        {
            foreach (double line in lines)
                if (line == value)
                    return true;
            return false;
        }


        private static bool HasExtent(Rect rect) => !rect.IsEmpty && rect.Width > 0 && rect.Height > 0;
    }
}
