using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Optimax
{
    public class ResizingAdorner : Adorner
    {
        public const int ADORNER_SIZE = 8;


        #region Fields

        // Resizing adorner uses Thumbs for visual elements.
        // The Thumbs have built-in mouse input handling.
        Thumb topLeft, topRight, bottomLeft, bottomRight;
        Thumb topHandle, bottomHandle, leftHandle, rightHandle;
        Rectangle rectangle;

        //Viewbox viewbox;
        private Size originalSize;

        // To store and manage the adorner's visual children.
        VisualCollection visualChildren;

        #endregion


        #region Properties

        public Brush Background { get; set; }

        public event Action<UIElement, Size> ResizeCompleted;

        #endregion


        #region C'tors

        // Initialize the ResizingAdorner.
        public ResizingAdorner(UIElement adornedElement)
            : this(adornedElement,new SolidColorBrush(Colors.Chartreuse))
        {
        }


        public ResizingAdorner(UIElement adornedElement, Brush thumbColor): base(adornedElement)
        {
            Background = thumbColor;

            visualChildren = new VisualCollection(this);

            BuildAdornerRectangle(ref rectangle);

            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);

            BuildAdornerCorner(ref topHandle, Cursors.SizeNS);
            BuildAdornerCorner(ref bottomHandle, Cursors.SizeNS);
            BuildAdornerCorner(ref leftHandle, Cursors.SizeWE);
            BuildAdornerCorner(ref rightHandle, Cursors.SizeWE);

            // Add handlers for resizing.
            bottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            topLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            topRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);

            topHandle.DragDelta += new DragDeltaEventHandler(HandleTop);
            bottomHandle.DragDelta += new DragDeltaEventHandler(HandleBottom);
            leftHandle.DragDelta += new DragDeltaEventHandler(HandleLeft);
            rightHandle.DragDelta += new DragDeltaEventHandler(HandleRight);
        }

        #endregion


        #region Resizing Handlers

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {


            Debug.WriteLine("Adorner.OnPreviewLeftMouseButtonDown");
            base.OnPreviewMouseLeftButtonDown(e);

            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            if (adornedElement == null)
                return;

            originalSize = new Size(adornedElement.Width,adornedElement.Height);

            //if (adornedElement.LayoutTransform.Equals(Transform.Identity))
            //    adornedElement.LayoutTransform = new ScaleTransform(1,1,0,0);

            //FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            //Canvas parentCanvas = parentElement as Canvas;
            //if (parentCanvas != null)
            //{
            //    Canvas canvas = adornedElement as Canvas;
            //    if (canvas == null)
            //        return;

            //    int parentIndex = parentCanvas.Children.IndexOf(canvas);
            //    Point pos = new Point(Canvas.GetLeft(canvas), Canvas.GetTop(canvas));

            //    viewbox = new Viewbox();
            //    parentCanvas.Children.RemoveAt(parentIndex);
            //    viewbox.Child = canvas;
            //    parentCanvas.Children.Insert(parentIndex, viewbox);

            //    viewbox.Width = canvas.Width;
            //    viewbox.Height = canvas.Height;

            //    Canvas.SetLeft(viewbox, pos.X);
            //    Canvas.SetTop(viewbox, pos.Y);
            //}

        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {


            //Debug.WriteLine("Adorner.OnPreviewLeftMouseButtonUp");

            //base.OnPreviewMouseLeftButtonUp(e);
            //if (viewbox == null)
            //   return;

            //FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            //if (adornedElement == null)
            //    return;
            //FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;
            //if (parentElement != viewbox)
            //    return;
            //parentElement = viewbox.Parent as FrameworkElement;
            //Canvas parentCanvas = parentElement as Canvas;
            //if (parentCanvas != null)
            //{
            //    Canvas canvas = viewbox.Child as Canvas;
            //    if (canvas == null)
            //        return;
            //    viewbox.Child = null;

            //    int parentIndex = parentCanvas.Children.IndexOf(viewbox);
            //    Point pos = new Point(Canvas.GetLeft(viewbox), Canvas.GetTop(viewbox));

            //    parentCanvas.Children.RemoveAt(parentIndex);
            //    parentCanvas.Children.Insert(parentIndex, canvas);

            //    Canvas.SetLeft(canvas, pos.X);
            //    Canvas.SetTop(canvas, pos.Y);
            //}


        }

        // Handler for resizing from the bottom-right.
        void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            Canvas canvas = adornedElement as Canvas;

            if (canvas == null)
            {
                // Ensure that the Width and Height are properly initialized after the resize.
                EnforceSize(adornedElement);

                // Change the size by the amount the user drags the mouse, as long as it's larger
                // than the width or height of an adorner, respectively.
                adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
                adornedElement.Height = Math.Max(adornedElement.Height + args.VerticalChange, hitThumb.DesiredSize.Height);
            }
            else
            {
                Size newSize = new Size(adornedElement.Width + args.HorizontalChange,adornedElement.Height + args.VerticalChange);

                double scaleX = newSize.Width / adornedElement.Width;
                double scaleY = newSize.Height / adornedElement.Height;

                double scale;
                if (args.HorizontalChange > args.VerticalChange)
                    scale = scaleY;
                else
                    scale = scaleX;

                Debug.WriteLine("HorizChange: {0}, Width: {1}, Scale: {2},{3}", args.HorizontalChange, adornedElement.Width, scaleX, scaleY);
                if (scaleX < 0)
                    Debug.WriteLine("!!!!");

                //Transform xfm = adornedElement.LayoutTransform;
                //adornedElement.LayoutTransform = new ScaleTransform(scaleX,scaleX,0,0);
                //((ScaleTransform)(adornedElement.LayoutTransform)).ScaleX = scaleX;
                //((ScaleTransform)(adornedElement.LayoutTransform)).ScaleY = scaleX;
                Point pos = new Point();
                foreach (FrameworkElement e in canvas.Children)
                {
                    pos.X = Canvas.GetLeft(e);
                    pos.Y = Canvas.GetTop(e);
                    e.Width = e.Width*scaleX;
                    e.Height = e.Height*scaleY;
                    Canvas.SetLeft(e,pos.X*scaleX);
                    Canvas.SetTop(e,pos.Y*scaleY);
                }

                adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
                adornedElement.Height = Math.Max(adornedElement.Height + args.VerticalChange, hitThumb.DesiredSize.Height);

                Debug.WriteLine("Element: {0},{1}", adornedElement.ActualWidth, adornedElement.ActualHeight);


                //adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
                //adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

                //if (viewbox != null)
                //{
                //    Point pos = new Point();
                //    pos.X = Canvas.GetLeft(viewbox);
                //    pos.Y = Canvas.GetTop(viewbox);

                //    Debug.WriteLine("Viewbox: {0}",pos);

                //    viewbox.Width = Math.Max(viewbox.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
                //    viewbox.Height = Math.Max(viewbox.Height + args.VerticalChange, hitThumb.DesiredSize.Height);
                //    //adornedElement.Width = Math.Max(viewbox.Width, hitThumb.DesiredSize.Width);
                //    //adornedElement.Height = Math.Max(viewbox.Height, hitThumb.DesiredSize.Height);
                //}
            }
        }

        // Handler for resizing from the top-right.
        void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            double height_old = adornedElement.Height;
            double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double top_old = Canvas.GetTop(adornedElement);
            adornedElement.Height = height_new;
            Canvas.SetTop(adornedElement, top_old - (height_new - height_old));
        }

        // Handler for resizing from the top-left.
        void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            double width_old = adornedElement.Width;
            double width_new = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double left_old = Canvas.GetLeft(adornedElement);
            adornedElement.Width = width_new;
            Canvas.SetLeft(adornedElement, left_old - (width_new - width_old));

            double height_old = adornedElement.Height;
            double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double top_old = Canvas.GetTop(adornedElement);
            adornedElement.Height = height_new;
            Canvas.SetTop(adornedElement, top_old - (height_new - height_old));
        }


        // Handler for resizing from the bottom-left.
        void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            double width_old = adornedElement.Width;
            double width_new = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double left_old = Canvas.GetLeft(adornedElement);
            adornedElement.Width = width_new;
            Canvas.SetLeft(adornedElement, left_old - (width_new - width_old));
        }



        // Handler for resizing from the bottom.
        void HandleBottom(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null)
                return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
        }


        // Handler for resizing from the top.
        void HandleTop(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null)
                return;
            //FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            // adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);

            if (ResizeSymmetric)
            {
                adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
                double height_old = adornedElement.Height;

                double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
                double top_old = Canvas.GetTop(adornedElement);
                double top_new = top_old - (height_new - height_old);
                adornedElement.Height = height_new;
                Canvas.SetTop(adornedElement, top_new);
            }
            else
            {
                double height_old = adornedElement.Height;
                double height_new = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
                double top_old = Canvas.GetTop(adornedElement);
                double top_new = top_old - (height_new - height_old);
                adornedElement.Height = height_new;
                Canvas.SetTop(adornedElement, top_new);
            }
        }

        // Handler for resizing from the left.
        void HandleLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null)
                return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger
            // than the width or height of an adorner, respectively.
            //adornedElement.Width = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            //adornedElement.Height = Math.Max(adornedElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);

            double width_old = adornedElement.Width;
            double width_new = Math.Max(adornedElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double left_old = Canvas.GetLeft(adornedElement);
            adornedElement.Width = width_new;
            Canvas.SetLeft(adornedElement, left_old - (width_new - width_old));
        }


        // Handler for resizing from the right.
        void HandleRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null)
                return;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
        }

        private void CornerThumbOnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;
            if (adornedElement == null || hitThumb == null)
                return;
            //Debug.WriteLine($"Resize Completed: {adornedElement.GetType()}. Original size: {originalSize}.");
            ResizeCompleted?.Invoke(adornedElement, originalSize);
        }

        #endregion


        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.
            // These will be used to place the ResizingAdorner at the corners of the adorned element.
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;

            topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            topHandle.Arrange(new Rect(desiredWidth/2-adornerWidth/2,-adornerHeight/2,adornerWidth,adornerHeight));
            bottomHandle.Arrange(new Rect(desiredWidth/2-adornerWidth/2, desiredHeight-adornerHeight/2, adornerWidth, adornerHeight));
            leftHandle.Arrange(new Rect(-adornerWidth/2, desiredHeight/2-adornerHeight/2, adornerWidth, adornerHeight));
            rightHandle.Arrange(new Rect(desiredWidth-adornerWidth/2, desiredHeight/2-adornerHeight/2, adornerWidth, adornerHeight));

            rectangle.Arrange(new Rect(0,0,desiredWidth,desiredHeight));

            // Return the final size.
            return finalSize;
        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property,
        // set some appearance properties, and add the elements to the visual tree.
        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            if (cornerThumb != null) return;

            cornerThumb = new Thumb();
            cornerThumb.DragCompleted += CornerThumbOnDragCompleted;

            // Set some arbitrary visual characteristics.
            cornerThumb.Cursor = customizedCursor;
            cornerThumb.Height = cornerThumb.Width = ADORNER_SIZE;
            cornerThumb.Opacity = 0.9;
            cornerThumb.Background = this.Background;
            cornerThumb.BorderThickness = new Thickness(0);

            visualChildren.Add(cornerThumb);
        }


        void BuildAdornerRectangle(ref Rectangle rectangle)
        {
            if (rectangle != null)
                return;

            rectangle = new Rectangle();
            rectangle.StrokeThickness = 1;
            rectangle.Stroke = Background;
            rectangle.Opacity = 0.9;
            rectangle.StrokeDashArray.Add(4);
            rectangle.StrokeDashArray.Add(2);


            visualChildren.Add(rectangle);
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            //FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            //if (parent != null)
            //{
            //    adornedElement.MaxHeight = parent.ActualHeight;
            //    adornedElement.MaxWidth = parent.ActualWidth;
            //}
        }


        // Override the VisualChildrenCount and GetVisualChild properties to interface with
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }




        public bool ResizeSymmetric {
            get { return false; }
        }


        //public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        //{

        //    topLeft.RenderTransform = new ScaleTransform(0.25, 0.25);
        //    topLeft.RenderTransformOrigin = new Point(0.5, 0.5);
        //    topRight.RenderTransform = new ScaleTransform(0.25, 0.25);
        //    topRight.RenderTransformOrigin = new Point(0.5, 0.5);
        //    bottomLeft.RenderTransform = new ScaleTransform(0.25, 0.25);
        //    bottomLeft.RenderTransformOrigin = new Point(0.5, 0.5);
        //    bottomRight.RenderTransform = new ScaleTransform(0.25, 0.25);
        //    bottomRight.RenderTransformOrigin = new Point(0.5, 0.5);

        //    return base.GetDesiredTransform(transform);
        //}


        //protected override Size MeasureOverride(Size constraint)
        //{
        //    var result = base.MeasureOverride(constraint);
        //    // ... add custom measure code here if desired ...
        //    InvalidateVisual();
        //    return result;
        //}

    }
}
