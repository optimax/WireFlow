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
        private double zoom;

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

            // Only the Designer defines this brush; hosts like the Browser set Background themselves.
            Background = TryFindResource("DesignerBackgroundBrush") as Brush;

            Canvas.RenderTransform = group;
            Canvas.RenderTransformOrigin = new Point(0.0, 0.0);

            Focusable = true;
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

            Zoom = 0;
        }



        #region Mouse Events

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            const double step = 0.1;
            var st = GetScaleTransform();
            var tt = GetTranslateTransform();

            double zoom = e.Delta > 0 ? step : -step;
            if (!(e.Delta > 0) && (st.ScaleX < .26 || st.ScaleY < .26))
                return;

            Point relative = e.GetPosition(Canvas);

            var abosuluteX = relative.X * st.ScaleX + tt.X;
            var abosuluteY = relative.Y * st.ScaleY + tt.Y;

            st.ScaleX += zoom;
            st.ScaleY += zoom;

            tt.X = abosuluteX - relative.X * st.ScaleX;
            tt.Y = abosuluteY - relative.Y * st.ScaleY;

            Zoom = st.ScaleX;
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
    }
}
