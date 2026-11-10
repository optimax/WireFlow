using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// Lets the items of a list of names be put in another order by dragging them: a drag that starts on an item and moves past
    /// the system's drag distance carries its name, a line shows where it goes, and the drop calls <c>move</c> with the name and
    /// the index it is to have once moved. A click without a drag, and a double click, stay the list's.
    /// </summary>
    public class ListDragReorder
    {
        private const string FORMAT = "WireFlow.ListDragReorder";

        private readonly ListBox list;
        private readonly Action<string, int> move;
        private Point start;
        private string pressed;
        private InsertionAdorner adorner;


        public ListDragReorder(ListBox list, Action<string, int> move)
        {
            this.list = list;
            this.move = move;
            list.AllowDrop = true;
            list.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
            list.PreviewMouseMove += PreviewMouseMove;
            list.DragOver += DragOver;
            list.DragLeave += (s, e) => RemoveAdorner();
            list.Drop += Drop;
        }


        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) as ListBoxItem;
            pressed = item?.Content as string;
            start = e.GetPosition(list);
        }


        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (pressed == null || e.LeftButton != MouseButtonState.Pressed || (list as EditableListBox)?.IsEditing == true)
                return;
            var offset = e.GetPosition(list) - start;
            if (Math.Abs(offset.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(offset.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            string name = pressed;
            pressed = null;
            try
            {
                DragDrop.DoDragDrop(list, new DataObject(FORMAT, name), DragDropEffects.Move);
            }
            finally
            {
                RemoveAdorner();
            }
        }


        private void DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(FORMAT))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
            int index = InsertionIndex(e.GetPosition(list), out double y);
            if (adorner == null)
            {
                adorner = new InsertionAdorner(list);
                AdornerLayer.GetAdornerLayer(list)?.Add(adorner);
            }
            adorner.Y = y;
            ScrollNear(index);
        }


        private void Drop(object sender, DragEventArgs e)
        {
            RemoveAdorner();
            if (e.Data.GetData(FORMAT) is not string name)
                return;
            e.Handled = true;
            int index = InsertionIndex(e.GetPosition(list), out _);
            int from = list.Items.IndexOf(name);
            if (from < 0)
                return;
            // The index it has once taken out of its place
            if (from < index)
                index--;
            if (index != from)
                move(name, index);
        }


        // The index of the gap nearest a point in the list (0 before the first item, Count after the last), and where the gap is
        private int InsertionIndex(Point point, out double y)
        {
            y = 0;
            int count = list.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem item || !item.IsVisible)
                    continue;
                double top = item.TranslatePoint(new Point(0, 0), list).Y;
                y = top + item.ActualHeight;
                if (point.Y < top + item.ActualHeight / 2)
                {
                    y = top;
                    return i;
                }
            }
            return count;
        }


        // Scrolls the list when the gap is at its edge, so that a page out of view can be reached
        private void ScrollNear(int index)
        {
            if (index > 0)
                list.ScrollIntoView(list.Items[Math.Min(index, list.Items.Count) - 1]);
            if (index < list.Items.Count)
                list.ScrollIntoView(list.Items[index]);
        }


        private void RemoveAdorner()
        {
            if (adorner == null)
                return;
            AdornerLayer.GetAdornerLayer(list)?.Remove(adorner);
            adorner = null;
        }


        // A line across the list, where the item dragged would go, in the list's text color so that it suits every theme
        private class InsertionAdorner : Adorner
        {
            private double y;

            public InsertionAdorner(UIElement list) : base(list)
            {
                IsHitTestVisible = false;
            }

            public double Y
            {
                get => y;
                set
                {
                    if (y == value)
                        return;
                    y = value;
                    InvalidateVisual();
                }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                var brush = (AdornedElement as Control)?.Foreground ?? SystemColors.HighlightBrush;
                double width = AdornedElement.RenderSize.Width;
                var pen = new Pen(brush, 2);
                drawingContext.DrawLine(pen, new Point(2, y), new Point(width - 2, y));
                drawingContext.DrawLine(new Pen(brush, 1), new Point(2, y - 3), new Point(2, y + 3));
                drawingContext.DrawLine(new Pen(brush, 1), new Point(width - 2, y - 3), new Point(width - 2, y + 3));
            }
        }
    }
}
