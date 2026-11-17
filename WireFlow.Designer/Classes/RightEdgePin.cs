using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// Holds its child at the right edge of the scrolling view around it, wherever the row it is in begins: the Outline's
    /// eye and padlock, which line up down the pane's right side though each entry starts at its own depth. The child is
    /// moved by a render transform, so the row is laid out as without it; <see cref="Room"/> is the width left before the
    /// child, which the row gives what it shows there as its greatest width, so that the two do not lie over each other.
    /// </summary>
    public class RightEdgePin : Decorator
    {
        private readonly TranslateTransform shift = new TranslateTransform();
        private ScrollContentPresenter view;

        private static readonly DependencyPropertyKey RoomKey =
            DependencyProperty.RegisterReadOnly(nameof(Room), typeof(double), typeof(RightEdgePin), new PropertyMetadata(double.PositiveInfinity));

        public static readonly DependencyProperty RoomProperty = RoomKey.DependencyProperty;


        /// <summary>The width from where the pin lies in its row to its child at the edge, less the gap; unbounded until it is placed.</summary>
        public double Room => (double)GetValue(RoomProperty);

        /// <summary>The room kept between the child and the edge.</summary>
        public double EdgeMargin { get; set; } = 2;

        /// <summary>The room kept between what is before the child and the child.</summary>
        public double Gap { get; set; } = 4;

        /// <summary>The least <see cref="Room"/>: in a view too narrow for it, the row goes on past the edge.</summary>
        public double LeastRoom { get; set; } = 40;


        public RightEdgePin()
        {
            RenderTransform = shift;
            Loaded += (sender, e) =>
            {
                view = FindView();
                LayoutUpdated -= OnLayoutUpdated;
                LayoutUpdated += OnLayoutUpdated;
                Place();
            };
            Unloaded += (sender, e) =>
            {
                LayoutUpdated -= OnLayoutUpdated;
                view = null;
            };
        }


        // The view that scrolls the row: its extent is what shows
        private ScrollContentPresenter FindView()
        {
            for (DependencyObject e = VisualTreeHelper.GetParent(this); e != null; e = VisualTreeHelper.GetParent(e))
                if (e is ScrollContentPresenter presenter)
                    return presenter;
            return null;
        }


        private void OnLayoutUpdated(object sender, EventArgs e) => Place();


        // Moves the child to the view's edge. The move changes no layout, and the room it gives the row changes the row's width
        // only, not where the row begins, so the next pass finds the same place and changes nothing
        private void Place()
        {
            if (view == null || !IsVisible || !(VisualTreeHelper.GetParent(this) is Visual holder) || !view.IsAncestorOf(holder))
                return;
            // Where the pin lies in the view without its shift: where its holder is, and its place in it
            double left = holder.TransformToAncestor(view).Transform(new Point(0, 0)).X + VisualTreeHelper.GetOffset(this).X;
            double wanted = Math.Max(view.ActualWidth - ActualWidth - EdgeMargin - left, LeastRoom + Gap);
            if (Math.Abs(wanted - shift.X) > 0.5)
                shift.X = wanted;
            double room = wanted - Gap;
            if (double.IsInfinity(Room) || Math.Abs(room - Room) > 0.5)
                SetValue(RoomKey, room);
        }
    }
}
