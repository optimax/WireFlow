using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WireFlow
{
    public enum MouseDragMode
    {
        None = -1,
        Select = 0,
        Pan,
        Navigate,
        Draw
    }


    /// <summary>
    /// Held inside DesignerNavigator:UserControl, provides mouse interaction logic for the designer canvas it holds.
    /// </summary>
    public class DesignerPanel : Border, INotifyPropertyChanged
    {
        private const string NAME_DESIGNER_CANVAS = "Background";

        private Point origin;
        private Point start;
        private double zoom = 1.0;

        private bool mouseIsDown;

        #region Properties


        public MouseDragMode Mode { get; set; }

        public bool IsModified { get; set; }

        public Canvas Canvas { get; }

        public bool MouseIsDown => mouseIsDown;

        public double Zoom
        {
            get { return zoom; }
            set
            {
                if (Math.Abs(zoom - value) < Constants.EPSILON)
                    return;
                zoom = value;
                ZoomLevelChanged?.Invoke(this, zoom);
                OnPropertyChanged(nameof(Zoom));
            }
        }


        #endregion


        #region Events


        public event Action<DesignerPanel, Point, FrameworkElement> SelectStart;
        public event Action<DesignerPanel, Point> SelectMove;
        public event Action<DesignerPanel, Point> SelectEnd;


        public event Action<DesignerPanel, Point> PanStart;
        public event Action<DesignerPanel, Point> PanMove;
        public event Action<DesignerPanel, Point> PanEnd;


        public event Action<DesignerPanel, Point> DrawStart;
        public event Action<DesignerPanel, Point> DrawMove;
        public event Action<DesignerPanel, Point> DrawEnd;


        //public event Action<DesignerPanel, Point, FrameworkElement> NavigateHover;
        public event Action<DesignerPanel, Point, FrameworkElement> NavigateClick;

        public event Action<DesignerPanel, double> ZoomLevelChanged;


        #endregion


        #region C'tor


        public DesignerPanel()
        {
            this.ClipToBounds = true;
            Canvas = new DesignerCanvas();
            base.Child = Canvas;
            Canvas.Name = NAME_DESIGNER_CANVAS;

            TransformGroup group = new TransformGroup();

            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            // Only the Designer defines these brushes, and changes them with the theme; hosts like the Browser set them themselves.
            SetResourceReference(BackgroundProperty, "DesignerBackgroundBrush");
            SetResourceReference(PageFrameBrushProperty, "PageFrameBrush");

            Canvas.RenderTransform = group;
            Canvas.RenderTransformOrigin = new Point(0.0, 0.0);

            // The frame follows the page as it is zoomed, panned and resized
            st.Changed += (s, e) => InvalidateVisual();
            tt.Changed += (s, e) => InvalidateVisual();
            Canvas.SizeChanged += (s, e) => InvalidateVisual();

            Focusable = true;
        }



        #endregion




        #region Page Frame

        public static readonly DependencyProperty PageFrameBrushProperty = DependencyProperty.Register(nameof(PageFrameBrush), typeof(Brush),
            typeof(DesignerPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// The 1 px frame drawn around the page, outside it, so that the page shows against the area around it whatever their colors.
        /// </summary>
        public Brush PageFrameBrush
        {
            get { return (Brush)GetValue(PageFrameBrushProperty); }
            set { SetValue(PageFrameBrushProperty, value); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            var brush = PageFrameBrush;
            if (brush == null || Canvas.ActualWidth <= 0 || Canvas.ActualHeight <= 0 || !IsAncestorOf(Canvas))
                return;

            // The page's bounds after zoom and pan, and a frame 1 device pixel wide just outside them
            Rect page = Canvas.TransformToAncestor(this).TransformBounds(new Rect(Canvas.RenderSize));
            var dpi = VisualTreeHelper.GetDpi(this);
            double width = 1.0 / dpi.DpiScaleX, height = 1.0 / dpi.DpiScaleY;
            Rect frame = new Rect(page.X - width / 2, page.Y - height / 2, page.Width + width, page.Height + height);
            var guidelines = new GuidelineSet(new[] { frame.Left - width / 2, frame.Right + width / 2 }, new[] { frame.Top - height / 2, frame.Bottom + height / 2 });
            dc.PushGuidelineSet(guidelines);
            dc.DrawRectangle(null, new Pen(brush, width), frame);
            dc.Pop();
        }

        #endregion




        #region Transforms

        public TranslateTransform GetTranslateTransform()
        {
            return (TranslateTransform)((TransformGroup)Canvas.RenderTransform).Children.First(tr => tr is TranslateTransform);
        }

        public ScaleTransform GetScaleTransform()
        {
            return (ScaleTransform)((TransformGroup)Canvas.RenderTransform).Children.First(tr => tr is ScaleTransform);
        }


        #endregion


        public void Reset()
        {
            // reset zoom
            var st = GetScaleTransform();
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;

            // reset pan
            var tt = GetTranslateTransform();
            tt.X = 0.0;
            tt.Y = 0.0;

            Zoom = 1.0;
        }




        #region Zoom

        /// <summary>The least zoom: 10%, so that Fit Page can show a long page whole.</summary>
        public const double MinimumZoom = 0.1;

        /// <summary>The greatest zoom: 500%.</summary>
        public const double MaximumZoom = 5.0;

        /// <summary>The step of Zoom In and Zoom Out and of a notch of the mouse wheel: 10%.</summary>
        public const double ZoomStep = 0.1;

        /// <summary>The room Fit Page leaves around the page, in pixels of the panel.</summary>
        public const double FitPageMargin = 20;

        /// <summary>The page's scale on the panel: 1 is its actual size.</summary>
        public double Scale => GetScaleTransform().ScaleX;


        /// <summary>
        /// Zooms to <paramref name="scale"/>, rounded to a whole percent and kept between <see cref="MinimumZoom"/> and
        /// <see cref="MaximumZoom"/>, keeping the point of the page under <paramref name="anchor"/> (a point of the panel) where it
        /// is: the mouse for the wheel, the center of the panel for the menu, the keys and the toolbar.
        /// </summary>
        public void ZoomTo(double scale, Point? anchor = null)
        {
            scale = Math.Clamp(Math.Round(scale * 100) / 100, MinimumZoom, MaximumZoom);
            var st = GetScaleTransform();
            var tt = GetTranslateTransform();
            Point at = anchor ?? new Point(ActualWidth / 2, ActualHeight / 2);

            // The page is drawn at its place in the panel's layout (offset), scaled, then moved by the pan: p * scale + tt + offset
            Vector offset = VisualTreeHelper.GetOffset(Canvas);
            double pageX = (at.X - offset.X - tt.X) / st.ScaleX;
            double pageY = (at.Y - offset.Y - tt.Y) / st.ScaleY;

            st.ScaleX = scale;
            st.ScaleY = scale;
            tt.X = at.X - offset.X - pageX * scale;
            tt.Y = at.Y - offset.Y - pageY * scale;

            Zoom = scale;
        }


        /// <summary>Zooms in or out by <paramref name="step"/> (<see cref="ZoomStep"/> or its negative) about the center of the panel.</summary>
        public void ZoomBy(double step) => ZoomTo(Scale + step);


        /// <summary>Shows the page at its actual size, 100%, keeping the point at the center of the panel where it is.</summary>
        public void ActualSize() => ZoomTo(1.0);


        /// <summary>
        /// Zooms so that the whole page shows, as large as the panel allows with <see cref="FitPageMargin"/> around it, and centers it.
        /// </summary>
        public void FitPage()
        {
            double width = Canvas.ActualWidth, height = Canvas.ActualHeight;
            double roomX = ActualWidth - 2 * FitPageMargin, roomY = ActualHeight - 2 * FitPageMargin;
            if (width <= 0 || height <= 0 || roomX <= 0 || roomY <= 0)
                return;

            double scale = Math.Clamp(Math.Floor(Math.Min(roomX / width, roomY / height) * 100) / 100, MinimumZoom, MaximumZoom);
            var st = GetScaleTransform();
            var tt = GetTranslateTransform();
            Vector offset = VisualTreeHelper.GetOffset(Canvas);

            st.ScaleX = scale;
            st.ScaleY = scale;
            tt.X = (ActualWidth - width * scale) / 2 - offset.X;
            tt.Y = (ActualHeight - height * scale) / 2 - offset.Y;

            Zoom = scale;
        }

        #endregion




        #region Mouse Events

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // A notch zooms in or out by a step, keeping the point under the mouse where it is
            ZoomTo(Scale + (e.Delta > 0 ? ZoomStep : -ZoomStep), e.GetPosition(this));
        }



        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(Canvas);
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos} {Mode} Source:{e.Source} OriginalSource:{e.OriginalSource}");

            base.OnPreviewMouseLeftButtonDown(e);
            Focus();
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Focus();
            FrameworkElement clickedElement;
            switch (Mode)
            {
                case MouseDragMode.Select:
                    start = e.GetPosition(Canvas);
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {start} {Mode} {e.Source}");
                    clickedElement = e.Source as FrameworkElement;
                    if (clickedElement == null)
                    {
                        var textRun = e.Source as Run;
                        clickedElement = textRun?.Parent as FrameworkElement;
                    }
                    SelectStart?.Invoke(this, start, clickedElement);
                    CaptureMouse();
                    mouseIsDown = true;
                    break;
                case MouseDragMode.Draw:
                    start = e.GetPosition(Canvas);
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {start} {Mode}");
                    DrawStart?.Invoke(this, start);
                    CaptureMouse();
                    mouseIsDown = true;
                    break;
                case MouseDragMode.Pan:
                    start = e.GetPosition(this);
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {start} {Mode}");
                    var tt = GetTranslateTransform();
                    origin = new Point(tt.X, tt.Y);
                    CaptureMouse();
                    mouseIsDown = true;
                    PanStart?.Invoke(this, start);
                    break;
                case MouseDragMode.Navigate:
                    start = e.GetPosition(Canvas);
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {start} {Mode}");
                    clickedElement = e.Source as FrameworkElement;
                    if (clickedElement != null)
                        NavigateClick?.Invoke(this, start, clickedElement);
                    break;
            }
        }



        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!MouseIsDown)
                return;
            Point mousePos;
            switch (Mode)
            {
                case MouseDragMode.Select:
                    mousePos = e.GetPosition(Canvas);
                    //Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos} {Mode}");
                    SelectMove?.Invoke(this,mousePos);
                    break;
                case MouseDragMode.Draw:
                    mousePos = e.GetPosition(Canvas);
                    //Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos} {Mode}");
                    DrawMove?.Invoke(this, mousePos);
                    break;
                case MouseDragMode.Pan:
                    mousePos = e.GetPosition(this);
                    var tt = GetTranslateTransform();
                    Vector v = start - mousePos;
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                    mousePos = e.GetPosition(Canvas);
                    PanMove?.Invoke(this,mousePos);
                    break;
                 case MouseDragMode.Navigate:
                    break;
            }
        }


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!MouseIsDown)
                return;

            var mousePos = e.GetPosition(Canvas);
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos} {Mode}");

            switch (Mode)
            {
                case MouseDragMode.Select:
                    SelectEnd?.Invoke(this,mousePos);
                    ReleaseMouseCapture();
                    mouseIsDown = false;
                    break;
                case MouseDragMode.Draw:
                    DrawEnd?.Invoke(this, mousePos);
                    ReleaseMouseCapture();
                    mouseIsDown = false;
                    break;
                case MouseDragMode.Pan:
                    PanEnd?.Invoke(this,mousePos);
                    ReleaseMouseCapture();
                    mouseIsDown = false;
                    break;
            }
        }


        #endregion


        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }



    public class DesignerCanvas : Canvas
    {
        public string AbsoluteId => this.GetHashCode().ToString();

        /// <summary>Not listed in Properties, as for the panels on the page (BaseElement): it would take the page's children.</summary>
        [Browsable(false)]
        public new bool IsItemsHost
        {
            get => base.IsItemsHost;
            set => base.IsItemsHost = value;
        }
    }
}
