using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MaxServices.Core.Controls;
using MaxServices.Core.Models;
using OptimaxTools.Wpf;

namespace WireFlow
{
    /// <summary>
    /// The editor of the TextAlignment of a text or a box: the 9-point position chooser of PDFTools (MaxServices.Core's
    /// PagePositionControl), which sets where the text lies in its frame: across, its TextAlignment (and a box's
    /// TextHorizontalAlignment), and down, its TextVerticalAlignment. Justify shows as left.
    /// </summary>
    public class TextPositionPropertyEditor : PropertyEditor
    {
        private static readonly PageContentPosition[,] Positions =
        {
            { PageContentPosition.TopLeft, PageContentPosition.TopCenter, PageContentPosition.TopRight },
            { PageContentPosition.CenterLeft, PageContentPosition.Center, PageContentPosition.CenterRight },
            { PageContentPosition.BottomLeft, PageContentPosition.BottomCenter, PageContentPosition.BottomRight },
        };


        /// <summary>Makes it the editor of the TextAlignment of texts and boxes in the property grid.</summary>
        public static void Register()
        {
            var editor = new TextPositionPropertyEditor();
            PropertyEditors.Register(typeof(TextAlignment), typeof(WireFlowTextBlock), nameof(TextBlock.TextAlignment), editor);
            PropertyEditors.Register(typeof(TextAlignment), typeof(Box), nameof(Box.TextAlignment), editor);
        }


        public override FrameworkElement CreateEditor(PropertyItem item)
        {
            var chooser = new PagePositionControl
            {
                ShowLabel = true,
                LabelPlacement = Dock.Right,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2),
                IsEnabled = !item.IsReadOnly,
            };
            chooser.SetBinding(PagePositionControl.SelectedPositionProperty,
                new Binding(nameof(Adapter.Position)) { Source = new Adapter(item), Mode = BindingMode.TwoWay });
            return chooser;
        }


        /// <summary>The position of the text in its frame, from its alignments across and down.</summary>
        public static PageContentPosition PositionOf(PropertyItem item)
        {
            var across = item.Value is TextAlignment a ? a : TextAlignment.Left;
            var down = TypeDescriptor.GetProperties(item.Component)["TextVerticalAlignment"]?.GetValue(item.Component) is VerticalAlignment v
                ? v
                : VerticalAlignment.Top;
            int column = across switch { TextAlignment.Center => 1, TextAlignment.Right => 2, _ => 0 };
            int row = down switch { VerticalAlignment.Center => 1, VerticalAlignment.Bottom => 2, _ => 0 };
            return Positions[row, column];
        }


        /// <summary>Sets the alignments across and down that put the text at a position of its frame.</summary>
        public static void Apply(PropertyItem item, PageContentPosition position)
        {
            for (int row = 0; row < 3; row++)
                for (int column = 0; column < 3; column++)
                {
                    if (Positions[row, column] != position)
                        continue;
                    var down = row switch { 1 => VerticalAlignment.Center, 2 => VerticalAlignment.Bottom, _ => VerticalAlignment.Top };
                    var block = column switch { 1 => HorizontalAlignment.Center, 2 => HorizontalAlignment.Right, _ => HorizontalAlignment.Left };
                    var across = column switch { 1 => TextAlignment.Center, 2 => TextAlignment.Right, _ => TextAlignment.Left };
                    item.SetComponentProperty("TextVerticalAlignment", down);
                    item.SetComponentProperty("TextHorizontalAlignment", block);
                    item.SetValue(across);
                    return;
                }
        }


        // The chooser's position as a property, which a binding follows without holding the chooser
        private sealed class Adapter : INotifyPropertyChanged
        {
            private readonly PropertyItem item;

            public event PropertyChangedEventHandler PropertyChanged;

            public Adapter(PropertyItem item)
            {
                this.item = item;
                item.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(PropertyItem.Value))
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                };
            }

            public PageContentPosition Position
            {
                get => PositionOf(item);
                set
                {
                    Apply(item, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                }
            }
        }
    }
}
