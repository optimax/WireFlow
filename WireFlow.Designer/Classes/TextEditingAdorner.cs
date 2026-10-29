using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{
    public class TextEditingAdorner: Adorner
    {

        // To store and manage the adorner's visual children.
        VisualCollection visualChildren;

        private readonly TextBox textBox;
        private Rect rect;
        private string origText;



        public string Text => textBox.Text;

        public event Action<TextEditingAdorner> CancelEditEvent;
        public event Action<TextEditingAdorner, string, string> CommitEditEvent;


        public TextEditingAdorner(UIElement adornedElement, Control control, Rect rect, string text): base(adornedElement)
        {
            visualChildren = new VisualCollection(this);
            this.rect = rect;
            origText = text;

            //rectangle = new Rectangle()
            //{
            //    Width = rect.Width,
            //    Height = rect.Height,
            //    Fill = Brushes.Orange,
            //    Stroke = Brushes.Aqua,
            //    StrokeThickness = 1,
            //};
            //visualChildren.Add(rectangle);

            textBox = new TextBox()
            {
                Width = rect.Width,
                Height = rect.Height,
                Text = text,
                Background = control.Background,
                Foreground = control.Foreground,
                FontSize = control.FontSize,
                FontFamily = control.FontFamily,
                FontWeight = control.FontWeight,
                FontStretch = control.FontStretch,
                FontStyle = control.FontStyle,
                Padding = new Thickness(2,1,2,1),
                CaretIndex = int.MaxValue,
                //AcceptsTab = true,
            };
            //textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewLostKeyboardFocus += TextBox_OnPreviewLostKeyboardFocus;
            textBox.KeyDown += TextBoxOnKeyDown;
            visualChildren.Add(textBox);

            textBox.Focus();
            textBox.SelectAll();
        }


        private void TextBoxOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.Escape:
                    CancelEditEvent?.Invoke(this);
                    break;
                case Key.Return:
                    CommitEditEvent?.Invoke(this, origText, Text);
                    break;
            }
        }


        private void TextBox_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs)
        {
            CancelEditEvent?.Invoke(this);
        }


        public new void Focus()
        {
            textBox.Focus();
        }


        // Override the VisualChildrenCount and GetVisualChild properties to interface with
        // the adorner's visual collection.
        protected override int VisualChildrenCount => visualChildren.Count;
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }



        protected override Size MeasureOverride(Size constraint)
        {
            textBox.Measure(constraint);
            return textBox.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            textBox.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(textBox.ActualWidth, textBox.ActualHeight);
        }


    }
}
