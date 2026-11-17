using Optimax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WireFlow
{
    /// <summary>
    /// Holds a DesignerPanel:Border, held inside a DesignerNavigatorDocument:LayoutDocument
    /// </summary>
    public partial class DesignerNavigator : UserControl, INotifyPropertyChanged
    {

        #region Dependencies

        private WireFlowEngine Engine { get; }
        private string CanvasBackground { get; set; }


        #endregion


        #region Constants

        private const double MIN_RUBBER_DELTA = 3.0;

        #endregion


        #region Fields

        public DesignerNavigatorDocument NavigatorDocument { get; set; }

        private IDrawingTool selectedDrawingTool;
        private readonly List<FrameworkElement> selectedElements = new List<FrameworkElement>();
        private FrameworkElement currentElement;
        private string pageName;
        private Layer currentLayer;

        #endregion


        #region Properties

        public string AbsoluteId => $"{GetHashCode()}-{Designer.GetHashCode()}-{Designer.Canvas.GetHashCode()}";

        public DesignerPanel Designer { get; set; }

        /// <summary>
        /// Whether the badge saying the Designer is in Navigate mode shows over the page. Its colors are the application's
        /// NavigateBadgeBrush and NavigateBadgeTextBrush, which Tools > Options sets.
        /// </summary>
        public bool ShowNavigateBadge
        {
            get { return navigateBadge.Visibility == Visibility.Visible; }
            set { navigateBadge.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }
        private Border navigateBadge;


        /// <summary>Raised when the cross at the end of the Navigate mode badge is clicked: the window then leaves Navigate mode.</summary>
        public event Action<DesignerNavigator> LeaveNavigateModeRequested;


        // A tab hanging from the top edge of the area, at its center, with its lower corners rounded: its words, and a cross
        // that leaves Navigate mode. Clicks go through it to the page under it, but for those on the cross.
        private Border CreateNavigateBadge()
        {
            var text = new TextBlock { Text = "Navigate Mode", FontWeight = FontWeights.SemiBold, FontSize = 12, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
            text.SetResourceReference(TextBlock.ForegroundProperty, "NavigateBadgeTextBrush");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(text);
            row.Children.Add(CreateNavigateBadgeCross());
            navigateBadge = new ClickThroughBorder
            {
                Child = row,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(16, 3, 8, 4),
                Visibility = Visibility.Collapsed,
            };
            navigateBadge.SetResourceReference(Border.BackgroundProperty, "NavigateBadgeBrush");
            return navigateBadge;
        }


        // The cross at the end of the badge, in the badge's text color: a click on it asks the window to leave Navigate mode.
        // Under the mouse it shows a round spot of that color, faint.
        private FrameworkElement CreateNavigateBadgeCross()
        {
            var spot = new System.Windows.Shapes.Ellipse { Opacity = 0 };
            spot.SetResourceReference(Shape.FillProperty, "NavigateBadgeTextBrush");
            var cross = new System.Windows.Shapes.Path
            {
                Width = 8,
                Height = 8,
                Stretch = Stretch.Uniform,
                StrokeThickness = 1.6,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            cross.SetResourceReference(System.Windows.Shapes.Path.DataProperty, "ToolGlyph.close");
            cross.SetResourceReference(Shape.StrokeProperty, "NavigateBadgeTextBrush");
            var button = new Grid
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Cursor = Cursors.Arrow,
                ToolTip = "Leave Navigate Mode (Esc)",
            };
            button.Children.Add(spot);
            button.Children.Add(cross);
            bool pressed = false;
            button.MouseEnter += (sender, e) => spot.Opacity = 0.3;
            button.MouseLeave += (sender, e) =>
            {
                spot.Opacity = 0;
                pressed = false;
            };
            button.MouseLeftButtonDown += (sender, e) =>
            {
                pressed = true;
                e.Handled = true;
            };
            button.MouseLeftButtonUp += (sender, e) =>
            {
                e.Handled = true;
                if (!pressed)
                    return;
                pressed = false;
                LeaveNavigateModeRequested?.Invoke(this);
            };
            return button;
        }


        // A border that mouse clicks go through to what lies under it, but for those on its content
        private sealed class ClickThroughBorder : Border
        {
            protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) => null;
        }


        private FlowBase _currentPage;

        public FlowBase CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (value == null)
                    Debugger.Break();
                _currentPage = value;
                StartHistory(value);
            }
        }


        // The Undo/Redo history of the page or layout open, since it was opened or last saved
        private FlowHistory history;
        // Whether a step of the history is open for a gesture of the mouse (a drag of the selection, a resize)
        private bool gestureStepOpen;
        // Whether the changes recorded outside a gesture are to be made a step once the edit making them is done
        private bool commitScheduled;

        public FlowHistory History => history;
        public bool CanUndo => history?.CanUndo == true;
        public bool CanRedo => history?.CanRedo == true;


        private Shape Rubberband { get; set; }
        private Point StartPoint { get; set; }
        private bool IsDraggingShapes { get; set; }

        // How near, in pixels on the screen, an edge has to come to a line for it to snap to it
        private const double SNAP_DISTANCE = 6;

        // A drag of the selection: where the mouse was pressed, where each selected element was then, whether the mouse
        // has gone far enough since for it to count as a drag, and what the elements snap to (null for nothing)
        private Point dragOrigin;
        private List<(FrameworkElement Element, Point Place)> dragPlaces = new List<(FrameworkElement Element, Point Place)>();
        private bool dragMoving;
        private SnapLines dragSnap;

        // What the element being resized snaps to
        private FrameworkElement resizeSnapElement;
        private SnapLines resizeSnap;

        // The lines that show what a moved or resized element has snapped to, down the page and across it
        private Line snapGuideAcross, snapGuideDown;
        private static readonly Brush SnapGuideBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x3D, 0xBE));


        // What a moved or resized element snaps to, in the coordinates of the panel holding it: the lines its left and
        // right edges snap to and those its top and bottom edges do, and, for a move, the bounds of what moves, before it does
        private sealed class SnapLines
        {
            public Panel Holder;
            public List<double> Across;
            public List<double> Down;
            public Rect Bounds;
        }


        public IDrawingTool SelectedDrawingTool
        {
            get { return selectedDrawingTool; }
            set
            {
                if (selectedDrawingTool == value)
                    return;
                selectedDrawingTool = value;
                ToolSelectionChanged(value);
                OnPropertyChanged(nameof(SelectedDrawingTool));
            }
        }


        public string PageName
        {
            get { return pageName; }
            set
            {
                if (pageName == value)
                    return;
                pageName = value;
                OnPageNameChanged(value);
                OnPropertyChanged(nameof(PageName));
            }
        }


        public MouseDragMode Mode
        {
            get { return Designer.Mode; }
            set { Designer.Mode = value; }
        }


        public Layer CurrentLayer
        {
            get { return currentLayer; }
            set
            {
                if (currentLayer == value)
                    return;
                currentLayer = value;

                //TODO: Notify of layer change
                OnPropertyChanged(nameof(CurrentLayer));
            }
        }





        /// <summary>The elements selected, the first one first.</summary>
        public IReadOnlyList<FrameworkElement> SelectedElements => selectedElements;


        public FrameworkElement CurrentElement
        {
            get { return currentElement; }
            set
            {
                if (currentElement == value)
                    return;
                currentElement = value;
                OnSelectionChanged(currentElement);
            }
        }


        /// <summary>Raised when the page or layout comes to have changes not saved, and when it no longer has.</summary>
        public event Action<DesignerNavigator> ModifiedChanged;


        public bool IsModified {
            get { return Designer.IsModified; }
            set
            {
                if (Designer.IsModified == value)
                    return;
                Designer.IsModified = value;
                ModifiedChanged?.Invoke(this);
            }
        }


        #endregion



        #region Computed Properties

        private Canvas CurrentCanvas => Designer.Canvas;

        public bool HasElementsSelected => selectedElements.Count > 0;
        public bool Has2PlusElementsSelected => selectedElements.Count >= 2;
        public bool Has3PlusElementsSelected => selectedElements.Count >= 3;
        public bool CanGroup => selectedElements.Count > 1;
        public bool CanUngroup => selectedElements.Count > 0;

        // An element inside a selected group or stack panel pressed on: selected when the press ends without a drag
        private FrameworkElement pendingChildSelection;

        private bool IsSelecting => Designer.Mode == MouseDragMode.Select;
        private bool IsPanning => Designer.Mode == MouseDragMode.Pan;
        private bool IsNavigating => Designer.Mode == MouseDragMode.Navigate;
        private bool IsDrawing => Designer.Mode == MouseDragMode.Draw;

        private bool IsCtrlPressed => KeyboardUtils.IsCtrlPressed();
        private bool IsShiftPressed => KeyboardUtils.IsShiftPressed();
        private bool IsModifierKeyPressed => IsShiftPressed || IsCtrlPressed;

        #endregion



        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<DesignerNavigator, string> PageNameChanged;
        public event Action<DesignerNavigator, FrameworkElement> SelectionChanged;
        public event Action<DesignerNavigator> ElementsChanged;
        /// <summary>Raised when the DOM shown changes, by an edit, an undo or a redo, or is replaced (opened, reloaded).</summary>
        public event Action<DesignerNavigator> DocumentChanged;


        private bool showLinkHighlights;
        private bool linkHighlightsScheduled;
        private bool watchingVisibility;
        // The overlays laid, with the adorner layer each is in, which takes it away even after its element left the page
        private readonly List<(AdornerLayer Layer, Adorner Adorner)> linkHighlights = new List<(AdornerLayer, Adorner)>();

        /// <summary>
        /// Whether every element on the page that is a link, its own or its layouts', has a highlight laid over it (View >
        /// Highlight Links). The highlights follow the page: they are laid again once each change is done.
        /// </summary>
        public bool ShowLinkHighlights
        {
            get => showLinkHighlights;
            set
            {
                if (showLinkHighlights == value)
                    return;
                showLinkHighlights = value;
                if (value && !watchingVisibility && Designer != null)
                {
                    // A tab behind another has no adorner layer: its highlights are laid when it comes to the front
                    watchingVisibility = true;
                    Designer.IsVisibleChanged += (sender, e) => ScheduleLinkHighlights();
                }
                ScheduleLinkHighlights();
            }
        }


        /// <summary>Lays the link highlights again (or takes them away) once the dispatcher has nothing left but input.</summary>
        public void ScheduleLinkHighlights()
        {
            if (linkHighlightsScheduled || Designer == null || (!showLinkHighlights && linkHighlights.Count == 0))
                return;
            linkHighlightsScheduled = true;
            Designer.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                linkHighlightsScheduled = false;
                UpdateLinkHighlights();
            }));
        }


        /// <summary>The number of link highlights laid on the page.</summary>
        public int LinkHighlightCount => linkHighlights.Count;


        private void UpdateLinkHighlights()
        {
            foreach (var (layer, adorner) in linkHighlights)
                layer.Remove(adorner);
            linkHighlights.Clear();
            var canvas = Designer?.Canvas;
            if (!showLinkHighlights || canvas == null)
                return;
            foreach (var element in LinkedElements(canvas))
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                if (layer == null)
                    continue;
                var adorner = new LinkHighlightAdorner(element);
                layer.Add(adorner);
                linkHighlights.Add((layer, adorner));
            }
        }


        // The elements shown that are links, at any depth: a group's or stack panel's children, and the layouts' elements too
        private static IEnumerable<FrameworkElement> LinkedElements(Panel panel)
        {
            foreach (var child in panel.Children.OfType<FrameworkElement>())
            {
                if (!child.IsVisible)
                    continue;
                // An element's link is its control's, which the engine keeps as its node has it
                if (child is ILinkable linked && !string.IsNullOrWhiteSpace(linked.Href))
                    yield return child;
                if (child is Panel inner)
                    foreach (var element in LinkedElements(inner))
                        yield return element;
            }
        }

        public event Action<DesignerNavigator, string> Navigating;
        //public event Action<DesignerNavigator, string> NavigationFailed;




        protected virtual void OnNavigating(string href)
        {
            Navigating?.Invoke(this, href);
        }


        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected virtual void OnPageNameChanged(string newName)
        {
            PageNameChanged?.Invoke(this, newName);
        }


        protected virtual void OnSelectionChanged(FrameworkElement element)
        {
            SelectionChanged?.Invoke(this,element);
        }


        /// <summary>
        /// Raised after elements are added to, removed from, or reordered within the page's element tree.
        /// </summary>
        protected virtual void OnElementsChanged()
        {
            ScheduleLinkHighlights();
            ElementsChanged?.Invoke(this);
        }


        #endregion



        #region Construction & Initialization

        public DesignerNavigator(WireFlowEngine engine, string background)
        {
            // By reference, so that the area around the page follows the theme
            SetResourceReference(BackgroundProperty, "DesignerBackgroundBrush");
            Engine = engine;
            CanvasBackground = background;
            Loaded += Window_OnLoaded;

            var contextMenu = FindResource("DesignerContextMenu") as ContextMenu;
            ContextMenu = contextMenu;

            this.KeyDown += KeyDownHandler;



            SetupDesigner();
        }



        private void SetupDesigner()
        {
            Designer = new DesignerPanel();
            // The page, with the Navigate mode badge over it, hanging from the top at the center
            var content = new Grid();
            content.Children.Add(Designer);
            content.Children.Add(CreateNavigateBadge());
            Content = content;

            //var content = Content as Grid;
            //if (Content == null)
            //    throw new NullReferenceException("Expecting Grid Content.");
            //var designer = content.Children[1] as DesignerPanelWrapper;
            //if (designer == null)
            //    throw new NullReferenceException("Expecting DesignerPanelWrapper in Grid.Children[1].");
            //Designer = designer;


            Designer.ZoomLevelChanged += ZoomLevelChangedHandler;
            Designer.PanMove += PanPositionChangedHandler;
            Designer.PanEnd += PanPositionChangedHandler;
            Designer.SelectStart += DesignerSelectStartHandler;
            Designer.SelectMove += DesignerSelectMoveHandler;
            Designer.SelectEnd += DesignerSelectEndHandler;
            Designer.DrawStart += DesignerDrawStartHandler;
            Designer.DrawMove += DesignerDrawMoveHandler;
            Designer.DrawEnd += DesignerDrawEndHandler;
            Designer.NavigateClick += DesignerNavigateClickHandler;
            Designer.PickClick += DesignerPickClickHandler;
        }



        private void Window_OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {Mode}");
        }



        //private void ResetCanvas(Canvas canvas, string colorStr = null)
        //{
        //    if (canvas == null)
        //        return;
        //    //CurrentDesigner?.Reset();
        //    canvas?.Children.Clear();

        //    canvas.Width = 0;
        //    canvas.Height = 0;

        //    Color fillColor = ColorUtils.StringToColor(colorStr, Constants.DEFAULT_BACKGROUND);
        //    Brush brush = new SolidColorBrush(fillColor);
        //    canvas.Background = brush;

        //    if (Engine.Project != null)
        //    {
        //        canvas.Width = Engine.Project.Width;
        //        canvas.Height = Engine.Project.Height;
        //    }
        //}

        #endregion



        #region Designer Events


        void ZoomLevelChangedHandler(DesignerPanel sender, double zoomLevel)
        {
            if (!HasElementsSelected)
                return;
            RebuildSelection();
        }


        void PanPositionChangedHandler(DesignerPanel sender, Point mousePos)
        {
            if (!HasElementsSelected)
                return;
            RebuildSelection();
        }


        public void DesignerSelectStartHandler(DesignerPanel designer, Point mousePos, FrameworkElement clickedElement)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos}");
            // A drag whose end was missed ends here
            EndGestureStep();
            pendingChildSelection = null;

            bool isSelected = selectedElements.Contains(clickedElement);
            bool isModifierKeyPressed = IsModifierKeyPressed;

            if (clickedElement == CurrentCanvas || clickedElement is DesignerPanel)
            {
                DeselectAll();
                IsDraggingShapes = false;
            }
            else
            {
                // If any element except canvas is clicked:
                // - if not selected, add the element to selection possibly clearing other selected elements
                // - if selected, remove from selection, if Shift or Ctrl are down

                if (!isSelected)
                {
                    // Inside a selected stack panel or group, a drag moves the selection; a click without one selects what was clicked (DesignerSelectEndHandler)
                    if (!isModifierKeyPressed && SelectedContainerOf(clickedElement) != null)
                        pendingChildSelection = clickedElement;
                    else
                    {
                        if (!isModifierKeyPressed)
                            DeselectAll();
                        SelectElementOrItsGroup(clickedElement);
                    }
                    IsDraggingShapes = true;
                }
                else
                {
                    if (isModifierKeyPressed)
                    {
                        IsDraggingShapes = false;
                        DeselectElementOrItsGroup(clickedElement);
                    }
                    else
                        IsDraggingShapes = true;
                }
            }
            StartPoint = mousePos;
            if (!IsDraggingShapes)
                CreateRubberband();
            else
            {
                BeginGestureStep();
                BeginDrag(mousePos);
            }
        }



        public void DesignerSelectMoveHandler(DesignerPanel designer, Point mousePos)
        {
            Point currentPoint = mousePos;

            if (IsDraggingShapes)
            {
                // The drag starts once the mouse has gone far enough. From then on the elements are where they were when the
                // mouse was pressed, plus how far it has gone since; where that brings an edge near a line it snaps to, on
                // it (measuring from the start, not from the last move, lets an element stay on a line while the mouse is near)
                if (!dragMoving)
                    dragMoving = Math.Abs(mousePos.X - dragOrigin.X) > SystemParameters.MinimumHorizontalDragDistance
                        || Math.Abs(mousePos.Y - dragOrigin.Y) > SystemParameters.MinimumVerticalDragDistance;
                if (dragMoving)
                {
                    pendingChildSelection = null;
                    var delta = mousePos - dragOrigin;
                    delta += SnapOffset(delta);
                    foreach (var (element, place) in dragPlaces)
                    {
                        SetLeft(element, place.X + delta.X);
                        SetTop(element, place.Y + delta.Y);
                    }
                    IsModified = true;
                }
            }
            else
                AdjustRubberband(currentPoint);
        }



        public void DesignerSelectEndHandler(DesignerPanel designer, Point mousePos)
        {
            Point currentPoint = mousePos;
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {currentPoint}");

            if (IsSelecting)
                SelectWithRubberband();
            RemoveRubberband();

            if (pendingChildSelection != null)
            {
                DeselectAll();
                SelectElementOrItsGroup(pendingChildSelection);
                pendingChildSelection = null;
            }

            IsDraggingShapes = false;
            EndDrag();
            EndGestureStep();
        }


        // The start of a drag of the selection: where each selected element is, and what they snap to
        private void BeginDrag(Point mousePos)
        {
            dragOrigin = mousePos;
            dragMoving = false;
            dragPlaces = selectedElements.Select(e => (e, BoundsOf(e).TopLeft)).ToList();
            dragSnap = SnapLinesFor(selectedElements.FirstOrDefault(e => !IsPlacedByItsPanel(e)), selectedElements);
        }


        private void EndDrag()
        {
            dragPlaces.Clear();
            dragMoving = false;
            dragSnap = null;
            HideSnapGuides();
        }


        // What an element snaps to as it is moved or resized with the mouse: the edges of the elements around it in the panel
        // holding it and of that panel (on a layer: of the elements of every visible layer and of the page's layouts, and
        // of the page), but those of the elements moving with it. Null for an element its panel places, which is not moved.
        private SnapLines SnapLinesFor(FrameworkElement e, IReadOnlyCollection<FrameworkElement> moving)
        {
            if (e == null || IsPlacedByItsPanel(e) || !(LogicalTreeHelper.GetParent(e) is Panel holder))
                return null;
            var others = Surroundings(e, holder, moving, out Rect container);
            var bounds = moving.Where(m => LogicalTreeHelper.GetParent(m) == holder && !IsPlacedByItsPanel(m)).Select(BoundsOf).Aggregate(BoundsOf(e), Rect.Union);
            return new SnapLines { Holder = holder, Across = SnapLayout.LinesAcross(container, others), Down = SnapLayout.LinesDown(container, others), Bounds = bounds };
        }


        // How far the elements dragged go beyond where the mouse alone puts them, so that an edge of theirs that has come
        // near a line lies on it; nothing while Ctrl is held. The lines they have snapped to are shown.
        private Vector SnapOffset(Vector delta)
        {
            if (dragSnap == null || IsCtrlPressed)
            {
                HideSnapGuides();
                return new Vector();
            }
            double distance = SNAP_DISTANCE / Designer.Scale;
            var bounds = dragSnap.Bounds;
            bounds.Offset(delta);
            double x = SnapLayout.SnapSpan(bounds.Left, bounds.Width, dragSnap.Across, distance, out double? across);
            double y = SnapLayout.SnapSpan(bounds.Top, bounds.Height, dragSnap.Down, distance, out double? down);
            ShowSnapGuide(ref snapGuideAcross, dragSnap.Holder, across, true);
            ShowSnapGuide(ref snapGuideDown, dragSnap.Holder, down, false);
            return new Vector(x, y);
        }


        // An edge of the element being resized, put on the nearest line it snaps to when one is near: one of its left and
        // right edges (across) or of its top and bottom ones, where the mouse asks it to be. It is left there while Ctrl is
        // held, and for an element its panel places.
        private double SnapResizedEdge(UIElement element, bool across, double edge)
        {
            var lines = IsCtrlPressed || !(element is FrameworkElement e) ? null : e == resizeSnapElement ? resizeSnap : SnapLinesFor(e, new[] { e });
            if (lines == null)
            {
                HideSnapGuides();
                return edge;
            }
            var snapLines = across ? lines.Across : lines.Down;
            double snapped = SnapLayout.Snap(edge, snapLines, SNAP_DISTANCE / Designer.Scale);
            ShowSnapGuide(ref across ? ref snapGuideAcross : ref snapGuideDown, lines.Holder, snapLines.Contains(snapped) ? snapped : (double?)null, across);
            return snapped;
        }


        // Shows the line an edge has snapped to: one down the page for an edge snapped across it, or one across the page;
        // the line is given in the coordinates of the panel holding the element. None takes the line away.
        private void ShowSnapGuide(ref Line guide, Panel holder, double? at, bool across)
        {
            if (at == null || CurrentCanvas == null)
            {
                if (guide != null)
                    CurrentCanvas?.Children.Remove(guide);
                guide = null;
                return;
            }
            if (guide == null)
            {
                guide = new Line { Stroke = SnapGuideBrush, IsHitTestVisible = false };
                Panel.SetZIndex(guide, int.MaxValue);
                CurrentCanvas.Children.Add(guide);
            }
            var origin = holder == null || !holder.IsDescendantOf(CurrentCanvas) ? new Point() : holder.TranslatePoint(new Point(), CurrentCanvas);
            guide.StrokeThickness = 1 / Designer.Scale;
            if (across)
            {
                guide.X1 = guide.X2 = at.Value + origin.X;
                guide.Y1 = 0;
                guide.Y2 = CurrentCanvas.Height;
            }
            else
            {
                guide.Y1 = guide.Y2 = at.Value + origin.Y;
                guide.X1 = 0;
                guide.X2 = CurrentCanvas.Width;
            }
        }


        private void HideSnapGuides()
        {
            ShowSnapGuide(ref snapGuideAcross, null, null, true);
            ShowSnapGuide(ref snapGuideDown, null, null, false);
        }



        public void DesignerDrawStartHandler(DesignerPanel designer, Point mousePos)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos}");
            //bool isModifierKeyPressed = IsModifierKeyPressed;
            StartPoint = mousePos;
            CreateRubberband();
        }


        public void DesignerDrawMoveHandler(DesignerPanel designer, Point mousePos)
        {
            Point currentPoint = mousePos;
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {currentPoint}");
            AdjustRubberband(mousePos);
        }


        public void DesignerDrawEndHandler(DesignerPanel designer, Point mousePos)
        {

            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {mousePos}");
            //bool isModifierKeyPressed = IsModifierKeyPressed;
            if (!IsShiftPressed)
                DeselectAll();
            //CreateElementFn?.Invoke();
            bool drawn = CreateShape(SelectedDrawingTool);
            RemoveRubberband();
            if (drawn)
                ElementDrawn?.Invoke(this);
        }


        /// <summary>Raised when the tool in use has drawn an object (Tools &gt; Options can then choose the Select tool again).</summary>
        public event Action<DesignerNavigator> ElementDrawn;




        // Follows the innermost link around the element clicked, its own first, then that of a group, stack panel or part
        // it is in, as the Browser does
        private void DesignerNavigateClickHandler(DesignerPanel designer, Point mousePos, FrameworkElement element)
        {
            string href = null;
            for (DependencyObject current = element; current != null && !(current is Layer); current = LogicalTreeHelper.GetParent(current))
                if (current is ILinkable link && !string.IsNullOrWhiteSpace(link.Href))
                {
                    href = link.Href;
                    break;
                }
            if (href == null)
                return;
            //TODO: Instead of navigating to page directly, ask the main window to do it, which in turn may call NavigateToPage, but then it may switch to an already open tab.
            // A link names the ID of a page, or of an element on one; one that names none leads nowhere
            var target = Engine.PageOfHref(href);
            if (target != null)
                OnNavigating(target);
            //NavigateToPage(href) is called by the Navigation Manager, not here!
        }

        // A click with the Pick Color tool; with Shift held, the colors given are kept for the next click
        public void DesignerPickClickHandler(DesignerPanel designer, Point mousePos, FrameworkElement element) => PickColor(element, IsShiftPressed);


        /// <summary>
        /// The Pick Color tool's click on an element: it takes the element's colors, its fill and its stroke, or, when the
        /// tool has colors taken, gives them to the element; the tool then forgets them, unless they are kept, as with Shift
        /// held, to be given to each element clicked after. The element is the innermost one around what was clicked that
        /// has a fill or a stroke.
        /// </summary>
        public void PickColor(FrameworkElement element, bool keep)
        {
            if (!(SelectedDrawingTool is PickColorDrawingTool pick))
                return;
            FlowElementBase node = null;
            for (DependencyObject current = element; current != null && !(current is Layer) && node == null; current = LogicalTreeHelper.GetParent(current))
                if (FlowView.GetNode(current) is FlowElementBase clicked && PickColorDrawingTool.HasColor(clicked))
                    node = clicked;
            if (node == null)
                return;
            if (!pick.HasColors)
                pick.Take(node);
            else
            {
                if (pick.GiveTo(node))
                    IsModified = true;
                if (!keep)
                    pick.Clear();
            }
            Designer.Cursor = pick.Cursor;
        }

        #endregion



        #region Navigation


        private bool navigatingAlready = false;



        public void NavigateToPage(string href)
        {
            if (navigatingAlready)
                return;// MessageBoxResult.None;
            if (string.IsNullOrWhiteSpace(href))
                return;// MessageBoxResult.None;

            Debug.WriteLine($"{this.GetType()}[{AbsoluteId}].{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name}-->{href}");

            SaveCurrentPage();
            //Debug.WriteLine($"Navigation {currentPage}-->{href} result: {result}");

            //if (result != MessageBoxResult.OK)
            //    NavigationFailed?.Invoke(href);

            //if (!result)
            //    OpenPageNode(currentPage);

            var result = OpenPageNode(href);
            if (result != MessageBoxResult.OK)
                return;
            // Tab name change: a page is named by its key, and its tab shows its name
            PageName = href;
            NavigatorDocument.Title = CurrentPage?.Name ?? href;


            ////TODO: Notify Pages Listbox of selection change
            ////navigatingAlready = true;
            ////var selPage = (string)ListBoxPages.SelectedItem;
            ////if (selPage != href)
            ////    ListBoxPages.SelectedItem = href;
            ////navigatingAlready = false;


            //CurrentElement = CurrentCanvas;
            ////HistoryIndex = 0;

            //return MessageBoxResult.OK;
        }



        public void NavigateToLayout(string href)
        {
            if (navigatingAlready)
                return;// MessageBoxResult.None;
            if (string.IsNullOrWhiteSpace(href))
                return;// MessageBoxResult.None;
            //Debug.WriteLine($"{this.GetType()}[{AbsoluteId}].{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name}-->{href}");

            SaveCurrentLayout();
            //Debug.WriteLine($"Navigation {currentPage}-->{href} result: {result}");

            //if (result != MessageBoxResult.OK)
            //    NavigationFailed?.Invoke(href);

            //if (!result)
            //    OpenPageNode(currentPage);

            var result = OpenLayoutNode(href);
            if (result != MessageBoxResult.OK)
                return;
            // Tab name change: a layout is named by its key, and its tab shows its name
            PageName = href;
            NavigatorDocument.Title = CurrentPage?.Name ?? href;


            ////TODO: Notify Pages Listbox of selection change
            ////navigatingAlready = true;
            ////var selPage = (string)ListBoxPages.SelectedItem;
            ////if (selPage != href)
            ////    ListBoxPages.SelectedItem = href;
            ////navigatingAlready = false;


            //CurrentElement = CurrentCanvas;
            ////HistoryIndex = 0;

            //return MessageBoxResult.OK;
        }



        public void NavigateToPart(string href)
        {
            if (navigatingAlready)
                return;
            if (string.IsNullOrWhiteSpace(href))
                return;
            SaveCurrentPart();
            var result = OpenPartNode(href);
            if (result != MessageBoxResult.OK)
                return;
            // Tab name change
            PageName = href;
            NavigatorDocument.Title = href;
        }


        /// <summary>
        /// Saves the page, layout or part this tab shows as what it is (its <see cref="DesignerNavigatorDocument.Kind"/>): a
        /// layout saved as a page would become a page of its name.
        /// </summary>
        public void SaveCurrentDocument()
        {
            switch (NavigatorDocument?.Kind)
            {
                case DesignerNavigatorDocument.LayoutKind:
                    SaveCurrentLayout();
                    break;
                case DesignerNavigatorDocument.PartKind:
                    SaveCurrentPart();
                    break;
                default:
                    SaveCurrentPage();
                    break;
            }
        }


        public void SaveCurrentPage()
        {
            if (CurrentPage == null || !IsModified)
                return;
            this.InvokeWithWaitCursor(() =>
            {
                Debug.WriteLine($"{this.GetType()}.{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name} PAGE SAVED");
                // Its DOM, which the edits changed
                Engine.SavePage(CurrentPage as FlowPage);
                IsModified = false;
                ClearHistory();
            });
        }



        public void SaveCurrentLayout()
        {
            if (CurrentPage == null || !IsModified)
                return;
            this.InvokeWithWaitCursor(() =>
            {
                Debug.WriteLine($"{this.GetType()}.{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name} LAYOUT SAVED");
                // Its DOM, which the edits changed
                Engine.SaveLayout(CurrentPage as FlowPage);
                IsModified = false;
                ClearHistory();
            });
        }


        public void SaveCurrentPart()
        {
            if (CurrentPage == null || !IsModified)
                return;
            this.InvokeWithWaitCursor(() =>
            {
                Debug.WriteLine($"{this.GetType()}.{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name} PART SAVED");
                var pageToSave = Engine.SerializePart(CurrentPage as FlowPart, CurrentCanvas);
                Engine.SavePart(pageToSave);
                IsModified = false;
                ClearHistory();
            });
        }



        private MessageBoxResult OpenPageNode(string nodeName, bool addToHistory = true)
        {
            try
            {
                Canvas canvas = CurrentCanvas;
                if (canvas == null)
                    return MessageBoxResult.None;
                if (string.IsNullOrWhiteSpace(nodeName))
                    return MessageBoxResult.None;

                //CurrentNode = null;
                //CurrentNodeName = nodeName;
                //OnPropertyChanged(nameof(NodeTitle));
                //if (addToHistory)
                //    AddToHistory(nodeName);


                CurrentPage = Engine.GetPage(nodeName);
                if (CurrentPage == null)
                {

                    //TODO: Offer to create a new page
                    //var msgResult = OfferToCreateNewPage(nodeName);
                    //if (msgResult != MessageBoxResult.Yes)
                    //    return msgResult;
                    return MessageBoxResult.No;
                }

                canvas.Background = ColorUtils.StringToBrush(Engine.Project.Background);
                Engine.RenderPage(canvas, CurrentPage as FlowPage);
                SelectDefaultLayer();

                Debug.WriteLine($"Rendered to Designer:{AbsoluteId} Canvas:{canvas.GetHashCode()}");


                //CurrentNode = node;
                //OnPropertyChanged(nameof(NodeTitle));

                //TODO: Populate Outline
                //PopulateOutlineView();



                IsModified = false;
                return MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
                return MessageBoxResult.None;
            }
        }



        private MessageBoxResult OpenLayoutNode(string nodeName)
        {
            try
            {
                Canvas canvas = CurrentCanvas;
                if (canvas == null)
                    return MessageBoxResult.None;
                if (string.IsNullOrWhiteSpace(nodeName))
                    return MessageBoxResult.None;


                CurrentPage = Engine.GetMasterPage(nodeName);
                if (CurrentPage == null)
                {

                    //TODO: Offer to create a new page
                    //var msgResult = OfferToCreateNewPage(nodeName);
                    //if (msgResult != MessageBoxResult.Yes)
                    //    return msgResult;
                    return MessageBoxResult.No;
                }

                canvas.Background = ColorUtils.StringToBrush(Engine.Project.Background);
                Engine.RenderPage(canvas, CurrentPage as FlowPage);
                SelectDefaultLayer();

                Debug.WriteLine($"Rendered to Designer:{AbsoluteId} Canvas:{canvas.GetHashCode()}");


                IsModified = false;
                return MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
                return MessageBoxResult.None;
            }
        }



        private MessageBoxResult OpenPartNode(string nodeName)
        {
            try
            {
                Canvas canvas = CurrentCanvas;
                if (canvas == null)
                    return MessageBoxResult.None;
                if (string.IsNullOrWhiteSpace(nodeName))
                    return MessageBoxResult.None;


                var part = Engine.GetPart(nodeName);
                if (part == null)
                {
                    //TODO: Offer to create a new page
                    //var msgResult = OfferToCreateNewPage(nodeName);
                    //if (msgResult != MessageBoxResult.Yes)
                    //    return msgResult;
                    return MessageBoxResult.No;
                }




                canvas.Background = ColorUtils.StringToBrush(Engine.Project.Background);
                Engine.RenderPage(canvas, CurrentPage as FlowPage);
                SelectDefaultLayer();

                Debug.WriteLine($"Rendered to Designer:{AbsoluteId} Canvas:{canvas.GetHashCode()}");


                IsModified = false;
                return MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
                return MessageBoxResult.None;
            }
        }


        #endregion


        #region Selection

        private void DeselectAll()
        {
            AdornerLayer aLayer;
            // Remove the adorner from each selected element
            foreach (var selectedElement in selectedElements)
            {
                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                if (aLayer != null)
                {
                    var adorners = aLayer.GetAdorners(selectedElement);
                    var adorner = adorners?.FirstOrDefault();
                    if (adorner != null)
                        aLayer.Remove(adorner);
                }
            }
            // deselect all
            selectedElements.Clear();
            CurrentElement = CurrentCanvas;
        }


        void RebuildSelection()
        {
            AdornerLayer aLayer;
            for (int i = 0; i < selectedElements.Count; i++)
            {
                UIElement selectedElement = selectedElements[i];
                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                var adorner = aLayer?.GetAdorners(selectedElement)?.FirstOrDefault();
                if (adorner != null)
                    aLayer?.Remove(adorner);
                AttachAdorner(selectedElement, aLayer, i > 0);
            }
            CurrentElement = selectedElements.FirstOrDefault();
        }



        void DeselectElementOrItsGroup(FrameworkElement clickedElement)
        {
            if (clickedElement == null)
                return;
            var lparent = LogicalTreeHelper.GetParent(clickedElement);
            Canvas parent = lparent as Canvas;
            if (parent == null)
                return;
            if (parent is Group)
            {
                //clickedElement = parent;
                DeselectElementOrItsGroup(parent);
            }
            else
            {
                DeselectElement(clickedElement);
            }

            Debug.WriteLine($"Deselected, {selectedElements.Count} elements still selected.");
        }


        // The layer an element is on, through the groups and stack panels holding it
        private static Layer LayerOf(FrameworkElement element)
        {
            for (var parent = LogicalTreeHelper.GetParent(element); parent != null; parent = LogicalTreeHelper.GetParent(parent))
                if (parent is Layer layer)
                    return layer;
            return null;
        }


        // The selected group, stack panel or panel holding the element, if any
        private FrameworkElement SelectedContainerOf(FrameworkElement element)
        {
            if (element == null)
                return null;
            for (var parent = LogicalTreeHelper.GetParent(element); parent is Group || parent is Stack || parent is AlignPanel; parent = LogicalTreeHelper.GetParent(parent))
                if (selectedElements.Contains(parent))
                    return (FrameworkElement)parent;
            return null;
        }


        /// <summary>Selects the groups, stack panels and panels holding the selected elements, as Esc does; with none, deselects all, which leaves the page selected.</summary>
        public void SelectContainers()
        {
            var containers = selectedElements.Select(e => LogicalTreeHelper.GetParent(e)).OfType<FrameworkElement>().Where(p => p is Group || p is Stack || p is AlignPanel).Distinct().ToList();
            DeselectAll();
            foreach (var container in containers)
                SelectElementOrItsGroup(container);
        }


        void SelectElementOrItsGroup(FrameworkElement clickedElement)
        {
            if (clickedElement == null)
                return;
            var lparent = LogicalTreeHelper.GetParent(clickedElement);
            Canvas parent = lparent as Canvas;
            if (parent == null)
                return;

            if ((clickedElement is Part) || (parent is Layer) || (parent is Stack) || (parent is AlignPanel) || (parent == CurrentCanvas)) // not in a group, or freehand
            {
                SelectElement(clickedElement);
                return;
            }

            //clickedElement = parent;
            SelectElementOrItsGroup(parent);

            Debug.WriteLine($"Selected {selectedElements.Count} elements.");
        }



        void SelectElement(FrameworkElement clickedElement)
        {
            if (LayerOf(clickedElement) != CurrentLayer || selectedElements.Contains(clickedElement))
                return;

            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(clickedElement);
            AttachAdorner(clickedElement, aLayer, HasElementsSelected);
            selectedElements.Add(clickedElement);
            CurrentElement = clickedElement;
        }



        void DeselectElement(FrameworkElement clickedElement)
        {
            selectedElements.Remove(clickedElement);
            var aLayer = AdornerLayer.GetAdornerLayer(clickedElement);
            var adorner = aLayer?.GetAdorners(clickedElement)?.FirstOrDefault();
            if (adorner != null)
            {

                (adorner as ResizingAdorner).ResizeStarted -= OnElementResizeStarted;
                (adorner as ResizingAdorner).ResizeCompleted -= OnElementResized;
                aLayer?.Remove(adorner);
            }
            // The selection changed even when the current element stays, as the Outline's highlights must know
            if (CurrentElement == clickedElement)
                CurrentElement = selectedElements.FirstOrDefault();
            else
                OnSelectionChanged(CurrentElement);
        }


        private void AttachAdorner(UIElement clickedElement, AdornerLayer aLayer, bool subsequent)
        {
            if (aLayer == null)
                return;

            ResizingAdorner adorner;
            if (subsequent)
                adorner = new ResizingAdorner(clickedElement, new SolidColorBrush(Colors.Yellow));
            else
                adorner = new ResizingAdorner(clickedElement, new SolidColorBrush(Colors.Chartreuse));
            adorner.ResizeStarted += OnElementResizeStarted;
            adorner.ResizeCompleted += OnElementResized;
            adorner.SnapEdge = SnapResizedEdge;
            adorner.IsClipEnabled = true;
            aLayer.Add(adorner);
        }



        private void OnElementResizeStarted(UIElement e)
        {
            BeginGestureStep();
            // What its edges snap to, for the length of the resize
            resizeSnapElement = e as FrameworkElement;
            resizeSnap = resizeSnapElement == null ? null : SnapLinesFor(resizeSnapElement, new[] { resizeSnapElement });
        }


        private void OnElementResized(UIElement e, Size s)
        {
            resizeSnapElement = null;
            resizeSnap = null;
            HideSnapGuides();
            IsModified = true;
            EndGestureStep();
        }



        #region Undo/Redo

        // Watches the DOM of the page or layout opened; what it held before is forgotten
        private void StartHistory(FlowBase page)
        {
            if (history != null)
            {
                history.Changed -= HistoryChanged;
                history.PropertyReplayed -= HistoryPropertyReplayed;
                history.Detach();
            }
            gestureStepOpen = false;
            history = page == null ? null : new FlowHistory(page);
            idsOutside = null;
            if (history != null)
            {
                history.Changed += HistoryChanged;
                history.NodeAdded += GiveIds;
                history.PropertyReplayed += HistoryPropertyReplayed;
            }
            OnHistoryStateChanged();
            DocumentChanged?.Invoke(this);
            ScheduleLinkHighlights();
        }


        // The IDs of the project's other documents, read once the page needs them
        private HashSet<string> idsOutside;


        // What the engine names the document by: a page by its key, a layout or part by its name
        private string DocumentKey => (CurrentPage as FlowPage)?.Key ?? CurrentPage?.Name;

        private HashSet<string> IdsOutside => idsOutside ??= Engine.IdsOutside(DocumentKey, CurrentPage is FlowPart);


        /// <summary>
        /// Whether another node than <paramref name="node"/> has the ID, on this page or layout (as it is now) or in the project's
        /// other documents; an empty ID is never taken. IDs compare ignoring case.
        /// </summary>
        public bool IsIdTaken(FlowBase node, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            if (FlowIds.Nodes(CurrentPage).Any(n => n != node && string.Equals(n.Id, id, StringComparison.OrdinalIgnoreCase)))
                return true;
            return IdsOutside.Contains(id) || Engine.IsIdGivenElsewhere(id, DocumentKey);
        }


        // An element put on the page without an ID, or with one another node has (a copy), gets one of its own: its kind and a number
        private void GiveIds(FlowBase added)
        {
            foreach (var node in FlowIds.Nodes(added).OfType<FlowElementBase>().ToList())
            {
                if (!string.IsNullOrWhiteSpace(node.Id) && !IsIdTaken(node, node.Id))
                    continue;
                var taken = FlowIds.NewSet();
                taken.UnionWith(IdsOutside);
                taken.UnionWith(Engine.IdsGivenElsewhere(DocumentKey));
                taken.UnionWith(FlowIds.IdsIn(CurrentPage));
                node.Id = FlowIds.ForKind(node.Type, taken);
                Engine.NoteIdGiven(node.Id, DocumentKey);
            }
        }


        private void ClearHistory()
        {
            gestureStepOpen = false;
            history?.Clear();
        }


        // Changes made outside a gesture make one step per edit: the step closes once the edit is done, when the dispatcher
        // has nothing left to do but wait for input, so an edit changing many nodes (aligning, grouping, ...) is undone at once
        private void HistoryChanged(object sender, EventArgs e)
        {
            if (sender == history && history.HasUncommittedChanges && !commitScheduled)
            {
                commitScheduled = true;
                var scheduledFor = history;
                Designer.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    commitScheduled = false;
                    if (scheduledFor == history)
                        history.CommitStep();
                    OnHistoryStateChanged();
                }));
            }
            OnHistoryStateChanged();
            DocumentChanged?.Invoke(this);
            ScheduleLinkHighlights();
        }


        private void OnHistoryStateChanged()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            // A step closed when idle comes with no input, after which the commands would otherwise be asked again
            CommandManager.InvalidateRequerySuggested();
        }


        private void BeginGestureStep()
        {
            if (gestureStepOpen || history == null)
                return;
            gestureStepOpen = true;
            history.BeginStep();
        }


        private void EndGestureStep()
        {
            if (!gestureStepOpen)
                return;
            gestureStepOpen = false;
            history?.EndStep();
        }


        // A layer taken away (by undo, say) is no longer current: the top one is
        private void KeepCurrentLayer()
        {
            if (CurrentLayer == null || CurrentLayer.Parent == null)
                SelectDefaultLayer();
        }


        // The layer drawn for a layer node of the page
        private Layer LayerViewOf(FlowLayer node) =>
            CurrentCanvas?.Children.OfType<Layout>().SelectMany(l => l.Children.OfType<Layer>()).FirstOrDefault(l => FlowView.GetNode(l) == node);


        /// <summary>Adds a layer to the page or layout open, above the current one, and makes it current.</summary>
        public void AddLayer()
        {
            // A part has no layers
            var page = CurrentPage as FlowPage;
            if (page == null)
                return;
            int n = page.Layers.Count + 1;
            while (page.Layers.Any(l => string.Equals(l.Name, $"Layer{n}", StringComparison.OrdinalIgnoreCase)))
                n++;
            var layer = new FlowLayer { Name = $"Layer{n}" };
            var current = FlowView.GetNode(CurrentLayer) as FlowLayer;
            int index = current != null && page.Layers.Contains(current) ? page.Layers.IndexOf(current) + 1 : page.Layers.Count;
            DeselectAll();
            page.Layers.Insert(index, layer);
            CurrentLayer = LayerViewOf(layer) ?? CurrentLayer;
            IsModified = true;
            OnElementsChanged();
        }


        /// <summary>
        /// Deletes a layer of the page or layout open, with its elements; false if it cannot: a page keeps one layer at least,
        /// and the layers of the layout a page shows are not the page's.
        /// </summary>
        public bool DeleteLayer(Layer view)
        {
            var page = CurrentPage as FlowPage;
            if (page == null || !(FlowView.GetNode(view) is FlowLayer layer) || !page.Layers.Contains(layer) || page.Layers.Count < 2)
                return false;
            DeselectAll();
            page.Layers.Remove(layer);
            KeepCurrentLayer();
            IsModified = true;
            OnElementsChanged();
            return true;
        }


        /// <summary>
        /// Makes the page or layout open what a version of it was: its size, its background, the layout it is set on, and
        /// its layers with their elements, which become the page's own (the version given is left without them). The
        /// page keeps its name and its ID. It is an edit, one step of Undo, after which the document has changes not saved.
        /// </summary>
        public bool RevertTo(FlowPage version)
        {
            if (!(CurrentPage is FlowPage page) || version == null || version == page)
                return false;
            EndGestureStep();
            // The elements selected are taken away
            DeselectAll();
            BeginGestureStep();
            try
            {
                page.Width = version.Width;
                page.Height = version.Height;
                page.Background = version.Background;
                page.Master = version.Master;
                var layers = version.Layers.ToList();
                version.Layers.Clear();
                page.Layers.Clear();
                foreach (var layer in layers)
                    page.Layers.Add(layer);
            }
            finally
            {
                EndGestureStep();
            }
            KeepCurrentLayer();
            IsModified = true;
            OnElementsChanged();
            return true;
        }


        /// <summary>
        /// Raised when an undo or a redo gave a node another ID, or the page or layout open another name: the node, the
        /// property (Id or Name), and its value before and after. The window then brings the project, its lists and tabs, and
        /// what names the node in the other documents, in line, as when the ID or name is set in Properties.
        /// </summary>
        public event Action<DesignerNavigator, FlowBase, string, string, string> TargetReplayed;


        private void HistoryPropertyReplayed(FlowBase node, string property, object before, object after)
        {
            if (property == nameof(FlowBase.Id) || (property == nameof(FlowBase.Name) && node == CurrentPage))
                TargetReplayed?.Invoke(this, node, property, before as string, after as string);
        }


        /// <summary>Undoes the last edit of the page or layout open. The page is unmodified again once every edit since it was opened or saved is undone.</summary>
        public void Undo()
        {
            if (!CanUndo)
                return;
            EndGestureStep();
            // The elements selected may be taken away
            DeselectAll();
            history.Undo();
            IsModified = history.CanUndo;
            KeepCurrentLayer();
            OnElementsChanged();
        }


        /// <summary>Redoes the last edit undone.</summary>
        public void Redo()
        {
            if (!CanRedo)
                return;
            EndGestureStep();
            DeselectAll();
            history.Redo();
            IsModified = true;
            KeepCurrentLayer();
            OnElementsChanged();
        }

        #endregion



        private void SelectAll()
        {
            DeselectAll();

            //TODO
        }



        private void SelectWithRubberband()
        {
            if (Rubberband == null)
                return;
            if (CurrentLayer == null)
                return;
            Rect bandRect = new Rect(Canvas.GetLeft(Rubberband), Canvas.GetTop(Rubberband), Rubberband.Width, Rubberband.Height);

            Debug.WriteLine($"Rubberband Canvas: {CurrentCanvas.GetHashCode()}");

            foreach (FrameworkElement item in CurrentLayer.Children)
            {
                // An element that does not show is not selected by the band drawn around where it would be
                if (item.Visibility != Visibility.Visible)
                    continue;
                Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
                Rect itemBounds = item.TransformToAncestor(CurrentCanvas).TransformBounds(itemRect);
                if (bandRect.Contains(itemBounds))
                    SelectElement(item);
            }

        }



        public void SelectItem(FrameworkElement element)
        {
            DeselectAll();

            var layer = element as Layer;
            if (layer != null)
            {
                var layout = layer as Layout;
                if (layout == null)
                // not a layout
                {
                    CurrentLayer = layer;
                    return;
                }
                // layout: select its first layer
                layer = layout.Children[0] as Layer;
                if (layer != null)
                    CurrentLayer = layer;
                return;
            }

            SelectElement(element);
        }



        #endregion



        #region Modes


        private void ResetAppModes()
        {
            Designer?.ReleaseMouseCapture();
            IsDraggingShapes = false;
            EndDrag();
            RemoveRubberband();

            //TODO: Select default tool in the toolbox
            //Tools.SelectedIndex = 0;
            //SelectTool(TOOL_SELECT);

            DeselectAll();
            IsDraggingShapes = false;
        }



        //private void SelectTool(int mode)
        //{
        //    SelectedToolIndex = mode;
        //}


        #endregion



        #region Keyboard


        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DeleteSelectedElements();
                    break;
                case Key.Escape:
                    //StickyCreate = false;
                    if (IsDraggingShapes || Designer?.IsMouseCaptured == true)
                        ResetAppModes();
                    else
                        SelectContainers();
                    break;
                case Key.Up:
                    MoveSelectionUp();
                    break;
                case Key.Down:
                    MoveSelectionDn();
                    break;
                case Key.Left:
                    MoveSelectionLeft();
                    break;
                case Key.Right:
                    MoveSelectionRight();
                    break;
            }
        }

        #endregion




        #region Creating and Destroying Shapes


        /// <summary>
        ///  Last layer of the last layout, i.e. topmost in z-order
        /// </summary>
        private void SelectDefaultLayer()
        {
            CurrentLayer = null;
            if (CurrentCanvas == null)
                return;
            if (CurrentCanvas.Children.Count == 0)
                return;
            var layout = CurrentCanvas?.Children[CurrentCanvas.Children.Count - 1] as Layout;
            if (layout == null || layout.Children.Count == 0)
                return;
            var defaultLayer = layout.Children[layout.Children.Count - 1] as Layer;
            CurrentLayer = defaultLayer;
        }


        private void ToolSelectionChanged(IDrawingTool tool)
        {
            var currentTool = tool;
            if (currentTool == null)
            {
                Debug.WriteLine("SelectedDrawingTool is NULL!");
                return;
            }

            Designer.Cursor = currentTool.Cursor;
            Designer.Mode = currentTool.DesignerMode;


            if (tool.DesignerMode == MouseDragMode.Navigate)
            {
                DeselectAll();
            }
            // The Pick Color tool starts with no colors taken, and nothing selected: a selected element's handles would
            // take the clicks meant for it
            if (tool is PickColorDrawingTool pick)
            {
                pick.Clear();
                Designer.Cursor = pick.Cursor;
                DeselectAll();
            }

            //switch (SelectedToolIndex)
            //{
            //    //case TOOL_SELECT:
            //    //    CreateElementFn = SelectWithRubberband;
            //    //    break;
            //    //case TOOL_PAN:
            //    //    break;
            //    //case TOOL_NAVIGATE:
            //    //    break;
            //    case TOOL_RECTANGLE:
            //        SetCreateFn<Rectangle>();
            //        break;
            //    case TOOL_ELLIPSE:
            //        SetCreateFn<Ellipse>();
            //        break;
            //    case TOOL_BOX:
            //        SetCreateFn<Box>();
            //        break;
            //    case TOOL_IMAGE:
            //        SetCreateFn<ImageBox>();
            //        break;
            //}
        }


        //private void SetCreateFn<ShapeType>()
        //    where ShapeType : FrameworkElement, new()
        //{
        //    CreateElementFn = CreateShape<ShapeType>;
        //    if (IsShiftPressed())
        //        StickyCreate = true;
        //}



        // Whether it drew an object
        bool CreateShape(IDrawingTool drawingTool)
        {
            if (drawingTool == null)
                return false;

            //SelectDefaultLayer(); DO NOT!
            if (CurrentLayer == null)
                return false;

            if (Rubberband == null)
                return false;

            // The tool makes the element with its defaults; it goes onto the page as its node, which the view draws
            var element = drawingTool.CreateElement();
            if (element == null)
                return false;
            var node = Engine.ToNode(element);
            if (node == null || !(FlowView.GetNode(CurrentLayer) is FlowLayer layer))
                return false;

            node.Width = Rubberband.Width;
            node.Height = Rubberband.Height;
            var start = new Point(Canvas.GetLeft(Rubberband), Canvas.GetTop(Rubberband));
            // An element drawn starting inside a stack panel becomes its last child, which the panel places; one drawn
            // starting inside a panel becomes its last child too, where it was drawn, until it is given an alignment
            Panel holder = FindPanelAt(CurrentLayer, start, out Point inside);
            if (holder != null && FlowView.GetNode(holder) is FlowGroup holderNode)
            {
                node.Left = holder is Stack ? 0 : inside.X;
                node.Top = holder is Stack ? 0 : inside.Y;
                holderNode.Children.Add(node);
                var drawn = ViewOf(node, holder);
                if (drawn != null)
                    SelectElementOrItsGroup(drawn);
            }
            else
            {
                node.Left = start.X;
                node.Top = start.Y;
                layer.Elements.Add(node);
                var drawn = ViewOf(node, CurrentLayer);
                if (drawn != null)
                    SelectElement(drawn);
            }

            IsModified = true;
            OnElementsChanged();
            return true;
        }


        /// <summary>
        /// The innermost stack panel or panel at a point of a layer or of one of them (in its coordinates), the topmost
        /// where they overlap, with the point in its own coordinates; null if there is none.
        /// </summary>
        private static Panel FindPanelAt(Panel container, Point point, out Point inside)
        {
            inside = point;
            foreach (var panel in container.Children.OfType<Panel>().Where(p => p is Stack || p is AlignPanel).Reverse())
            {
                var bounds = new Rect(Canvas.GetLeft(panel), Canvas.GetTop(panel), panel.ActualWidth, panel.ActualHeight);
                if (!bounds.Contains(point))
                    continue;
                var within = new Point(point.X - bounds.X, point.Y - bounds.Y);
                var inner = FindPanelAt(panel, within, out inside);
                if (inner != null)
                    return inner;
                inside = within;
                return panel;
            }
            return null;
        }



        void CreateShape<ShapeType>()
            where ShapeType : FrameworkElement, new()
        {
            if (!IsDrawing)
                return;
            if (Rubberband == null)
                return;

            var shape = new ShapeType();



            //shape.Stroke = new SolidColorBrush(Colors.Black);
            //shape.StrokeThickness = 2;
            //shape.Fill = Brushes.WhiteSmoke;


            SelectDefaultLayer();
            if (CurrentLayer == null)
                return;

            CurrentLayer.Children.Add(shape);

            shape.Width = 3;
            shape.Height = 3;


            shape.Width = Rubberband.Width;
            shape.Height = Rubberband.Height;
            Canvas.SetLeft(shape, Canvas.GetLeft(Rubberband));
            Canvas.SetTop(shape, Canvas.GetTop(Rubberband));

            SelectElement(shape);
            IsModified = true;
            OnElementsChanged();
        }




        public void DuplicateSelectedElements()
        {
            if (selectedElements.Count == 0)
                return;

            // A copy of each element's node, made through its JSON as it is saved, on top of the nodes beside it
            var copies = new List<(FlowElementBase Node, Panel Panel)>();
            foreach (var e in selectedElements)
            {
                var node = NodeOf(e);
                var siblings = SiblingsOf(node);
                if (siblings == null || !(LogicalTreeHelper.GetParent(e) is Panel parent))
                    continue;
                var copy = FlatFolderRepository.DeserializeFromJSON<FlowElementBase>(FlatFolderRepository.SerializeToJSON(node), node.Name);
                if (copy == null)
                    continue;
                siblings.Add(copy);
                copies.Add((copy, parent));
            }
            DeselectAll();
            selectedElements.AddRange(copies.Select(c => ViewOf(c.Node, c.Panel)).Where(v => v != null));
            RebuildSelection();

            IsModified = true;
            OnElementsChanged();
        }


        /// <summary>The clipboard format of WireFlow's elements: a JSON array of their nodes, as they are saved.</summary>
        public const string ClipboardFormat = "WireFlow.Elements";

        // How far a paste is moved while it would lie exactly on elements already there
        private const double PASTE_OFFSET = 10;


        /// <summary>
        /// Puts copies of the selected elements' nodes on the clipboard, each where it shows on the layer (a child of a group or a
        /// stack panel too), without the ones inside another selected; false when nothing is selected.
        /// </summary>
        public bool CopySelectedElements()
        {
            if (selectedElements.Count == 0 || CurrentLayer == null)
                return false;
            var chosen = selectedElements.Where(e => NodeOf(e) != null).ToList();
            chosen = chosen.Where(e => !chosen.Any(o => o != e && IsInsideOf(NodeOf(e), NodeOf(o)))).ToList();
            var copies = new List<string>();
            foreach (var e in chosen)
            {
                var node = NodeOf(e);
                var copy = FlatFolderRepository.DeserializeFromJSON<FlowElementBase>(FlatFolderRepository.SerializeToJSON(node), node.Name);
                if (copy == null)
                    continue;
                var place = e.TranslatePoint(new Point(0, 0), CurrentLayer);
                copy.Left = place.X;
                copy.Top = place.Y;
                copies.Add(FlatFolderRepository.SerializeToJSON(copy));
            }
            if (copies.Count == 0)
                return false;
            var data = new DataObject();
            data.SetData(ClipboardFormat, "[" + string.Join(",", copies) + "]");
            try
            {
                Clipboard.SetDataObject(data, true);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // The clipboard is held by another program
                return false;
            }
            return true;
        }


        /// <summary>Copies the selected elements to the clipboard and takes them off the page.</summary>
        public void CutSelectedElements()
        {
            if (CopySelectedElements())
                DeleteSelectedElements();
        }


        /// <summary>Whether the clipboard holds WireFlow elements to paste.</summary>
        public static bool ClipboardHasElements()
        {
            try
            {
                return Clipboard.ContainsData(ClipboardFormat);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return false;
            }
        }


        /// <summary>
        /// Puts the elements on the clipboard on the current layer, where they were copied from, or moved down and right by 10 px
        /// while they would lie exactly on elements of the same kind already there, and selects them. Each gets an ID of its own.
        /// </summary>
        public void PasteElements()
        {
            if (!(FlowView.GetNode(CurrentLayer) is FlowLayer layer))
                return;
            string json;
            try
            {
                json = Clipboard.GetData(ClipboardFormat) as string;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(json))
                return;
            List<FlowElementBase> nodes;
            try
            {
                nodes = Newtonsoft.Json.Linq.JArray.Parse(json)
                    .Select(item => FlatFolderRepository.DeserializeFromJSON<FlowElementBase>(item.ToString(), "clipboard"))
                    .Where(n => n != null).ToList();
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return;
            }
            if (nodes.Count == 0)
                return;
            bool Covers(double dx, double dy) => nodes.Any(n => layer.Elements.Any(o => o.Type == n.Type
                && Math.Abs(o.Left - (n.Left + dx)) < 0.5 && Math.Abs(o.Top - (n.Top + dy)) < 0.5));
            double offset = 0;
            while (Covers(offset, offset) && offset < 100 * PASTE_OFFSET)
                offset += PASTE_OFFSET;
            foreach (var node in nodes)
            {
                node.Left += offset;
                node.Top += offset;
            }

            DeselectAll();
            foreach (var node in nodes)
                layer.Elements.Add(node);
            selectedElements.AddRange(nodes.Select(n => ViewOf(n, CurrentLayer)).Where(v => v != null));
            RebuildSelection();

            IsModified = true;
            OnElementsChanged();
        }


        // Whether a node is held, at any depth, by another
        private static bool IsInsideOf(FlowBase node, FlowBase other)
        {
            for (var parent = node?.Parent; parent != null; parent = parent.Parent)
                if (parent == other)
                    return true;
            return false;
        }


        public void DeleteSelectedElements()
        {
            if (selectedElements.Count == 0)
                return;
            var nodes = selectedElements.Select(NodeOf).Where(n => n != null).ToList();
            // Deselected first, so that the adorners go while the elements are on the page
            DeselectAll();
            foreach (var node in nodes)
                SiblingsOf(node)?.Remove(node);

            IsModified = true;
            OnElementsChanged();
        }


        #endregion



        #region Grouping

        /// <summary>
        /// Group the selected elements
        /// </summary>
        public void GroupSelected()
        {
            if (!CanGroup)
                return;

            if (!(NodeOf(selectedElements[0])?.Parent is FlowLayer layer))
                return;
            var bounds = GetGroupBounds(selectedElements);
            // The selected elements of the anchor's layer, in their order on it, bottom first
            var nodes = selectedElements.Select(NodeOf).Where(n => n != null && n.Parent == layer).OrderBy(n => layer.Elements.IndexOf(n)).ToList();
            DeselectAll();

            var group = new FlowGroup { Type = "group", Left = bounds.Left, Top = bounds.Top, Width = bounds.Width, Height = bounds.Height };
            // Reparent and reposition the elements. The group is put on the layer first, and each element changed only while it is
            // on the page, where the Undo history sees its changes.
            layer.Elements.Add(group);
            foreach (var node in nodes)
            {
                layer.Elements.Remove(node);
                group.Children.Add(node);
                node.Left -= bounds.X;
                node.Top -= bounds.Y;
            }

            var drawn = ViewOf(group, CurrentLayer);
            if (drawn != null)
                SelectElement(drawn);

            IsModified = true;
            OnElementsChanged();
        }


        /// <summary>
        /// Expand Group Bounds
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private Rect GetGroupBounds(IEnumerable<FrameworkElement> list)
        {
            Rect bounds = new Rect(Double.MaxValue, Double.MaxValue, 0, 0);
            Rect pos = new Rect();
            foreach (FrameworkElement e in list)
            {
                pos.X = Canvas.GetLeft(e);
                pos.Y = Canvas.GetTop(e);
                // Use these as points within the loop
                pos.Width = pos.X + e.Width;
                pos.Height = pos.Y + e.Height;

                if (pos.X < bounds.X)
                    bounds.X = pos.X;
                if (pos.Y < bounds.Y)
                    bounds.Y = pos.Y;

                if (pos.Width > bounds.Width)
                    bounds.Width = pos.Width;
                if (pos.Height > bounds.Height)
                    bounds.Height = pos.Height;
            }
            // Translate point back to Width and Height
            bounds.Width = bounds.Width - bounds.X;
            bounds.Height = bounds.Height - bounds.Y;
            return bounds;
        }


        public void UngroupSelected()
        {
            var selection = selectedElements.ToList();
            DeselectAll();
            var nodes = new List<FlowElementBase>();
            foreach (FrameworkElement e in selection)
            {
                var node = NodeOf(e);
                // Groups, stack panels and panels; a part reference keeps its part
                if (node is FlowGroup group && !(node is FlowPart) && e is Panel panel)
                    UngroupNode(group, panel, nodes);
                else if (node != null)
                    nodes.Add(node);
            }
            foreach (var node in nodes)
            {
                var drawn = ViewOf(node, CurrentLayer);
                if (drawn != null)
                    SelectElement(drawn);
            }

            IsModified = true;
            OnElementsChanged();
        }


        /// <summary>
        /// Puts a group's, stack panel's or panel's children in its place in its layer, where they are on the page (as
        /// drawn, which is where a stack panel or a panel put them), and takes it away. A child released keeps no alignment.
        /// </summary>
        private static void UngroupNode(FlowGroup group, Panel panel, List<FlowElementBase> released)
        {
            if (!(group.Parent is FlowLayer layer))
                return;
            int index = layer.Elements.IndexOf(group);
            var children = group.Children.ToList();
            var places = children.Select(child => ViewOf(child, panel))
                .Select(v => v == null ? (Rect?)null : new Rect(Canvas.GetLeft(v), Canvas.GetTop(v), v.Width, v.Height)).ToList();
            // Each child is put on the layer, before the group, and then placed, and the group is taken away last, so that every
            // change is made on the page, where the Undo history sees it
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                group.Children.Remove(child);
                layer.Elements.Insert(index + i, child);
                if (places[i] is Rect place)
                {
                    child.Left = place.X;
                    child.Top = place.Y;
                    child.Width = place.Width;
                    child.Height = place.Height;
                }
                child.Left += group.Left;
                child.Top += group.Top;
                child.Alignment = FlowAlignment.None;
            }
            layer.Elements.Remove(group);
            released.AddRange(children);
        }


        #endregion


        #region Alignment & Distribution


        public void AlignTops()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                SetTop(anchorElement, 0);
                return;
            }
            var top = Canvas.GetTop(anchorElement);
            foreach (FrameworkElement e in selectedElements)
            {
                SetTop(e, top);
            }
            IsModified = true;
        }


        public void AlignBottoms()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                SetTop(anchorElement, CurrentCanvas.Height - anchorElement.Height);
                return;
            }
            var bottom = Canvas.GetTop(anchorElement) + anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
            {
                SetTop(e, bottom - e.Height);
            }
            IsModified = true;
        }


        public void AlignLefts()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                SetLeft(anchorElement, 0);
                return;
            }
            var left = Canvas.GetLeft(anchorElement);
            foreach (FrameworkElement e in selectedElements)
            {
                SetLeft(e, left);
            }
            IsModified = true;
        }


        public void AlignRights()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                SetLeft(anchorElement, CurrentCanvas.Width - anchorElement.Width);
                return;
            }
            var right = Canvas.GetLeft(anchorElement) + anchorElement.Width;
            foreach (FrameworkElement e in selectedElements)
            {
                SetLeft(e, right - e.Width);
            }
            IsModified = true;
        }



        public void AlignCentersHorizontally()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                var hCenter = CurrentCanvas.Width / 2;
                SetLeft(anchorElement, hCenter - anchorElement.Width / 2);
                return;
            }
            var center = Canvas.GetLeft(anchorElement) + anchorElement.Width / 2;
            foreach (FrameworkElement e in selectedElements)
            {
                SetLeft(e, center - (e.Width / 2));
            }
            IsModified = true;
        }


        public void AlignCentersVertically()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            if (selectedElements.Count == 1)
            {
                var vCenter = CurrentCanvas.Height / 2;
                SetTop(anchorElement, vCenter - anchorElement.Height / 2);
                return;
            }
            var center = Canvas.GetTop(anchorElement) + (anchorElement.Height / 2);
            foreach (FrameworkElement e in selectedElements)
            {
                SetTop(e, center - (e.Height / 2));
            }
            IsModified = true;
        }


        public void CenterInPage()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            var hCenter = CurrentCanvas.Width / 2;
            var vCenter = CurrentCanvas.Height / 2;
            if (selectedElements.Count == 1)
            {
                SetLeft(anchorElement, hCenter - anchorElement.Width / 2);
                SetTop(anchorElement, vCenter - anchorElement.Height / 2);
                return;
            }
            foreach (FrameworkElement e in selectedElements)
            {
                SetTop(e, vCenter - (e.Height / 2));
                SetLeft(e, hCenter - (e.Width / 2));
            }
            IsModified = true;
        }


        /// <summary>
        /// Makes each selected element fill the room around it, across and down: its edges go out to the nearest elements
        /// beside, above and under it, or to the edges of the page (see <see cref="Fill"/>).
        /// </summary>
        public void FillSpace() => Fill(true, true);


        /// <summary>Makes each selected element fill the room it has across: its left and right edges go out, its top and height stay.</summary>
        public void FillWidth() => Fill(true, false);


        /// <summary>Makes each selected element fill the room it has down: its top and bottom edges go out, its left and width stay.</summary>
        public void FillHeight() => Fill(false, true);


        // Makes each selected element fill the room it has in what holds it, across, down or both (FillLayout): the page,
        // where the elements of every layer and of the page's layouts take room, or its group, where the group's other
        // children do. One after another, so that two selected elements do not fill the same room. A stack panel's child
        // is left as it is, and so is a child a panel has aligned: the panel places and sizes it.
        private void Fill(bool across, bool down)
        {
            foreach (var e in selectedElements.ToList())
            {
                var holder = LogicalTreeHelper.GetParent(e) as Panel;
                if (holder == null || IsPlacedByItsPanel(e))
                    continue;
                var others = Surroundings(e, holder, null, out Rect container);
                var bounds = BoundsOf(e);
                var filled = FillLayout.Fill(bounds, container, others, across, down);
                if (filled == bounds)
                    continue;
                SetLeft(e, filled.Left);
                SetTop(e, filled.Top);
                SetWidth(e, filled.Width);
                SetHeight(e, filled.Height);
                IsModified = true;
            }
        }


        // The places of the elements around an element in the panel holding it, which take room there, and the room itself:
        // on a layer, the elements of every visible layer and of the page's layouts, in the page; in a group or a panel,
        // its other children, in it. The elements left out are not among them.
        private List<Rect> Surroundings(FrameworkElement e, Panel holder, IReadOnlyCollection<FrameworkElement> leftOut, out Rect container)
        {
            var panels = holder is Layer
                ? CurrentCanvas.Children.OfType<Layout>().SelectMany(l => l.Children.OfType<Layer>()).Where(l => l.Visibility == Visibility.Visible).Cast<Panel>()
                : new[] { holder };
            container = holder is Layer ? new Rect(0, 0, CurrentCanvas.Width, CurrentCanvas.Height) : new Rect(0, 0, holder.Width, holder.Height);
            return panels.SelectMany(p => p.Children.OfType<FrameworkElement>())
                .Where(o => o != e && (leftOut == null || !leftOut.Contains(o)) && o.Visibility == Visibility.Visible && FlowView.GetNode(o) != null)
                .Select(BoundsOf).ToList();
        }


        // An element's place in the panel holding it; one without a size of its own has the size it is drawn at
        private static Rect BoundsOf(FrameworkElement e)
        {
            double left = Canvas.GetLeft(e), top = Canvas.GetTop(e);
            double width = double.IsNaN(e.Width) ? e.ActualWidth : e.Width, height = double.IsNaN(e.Height) ? e.ActualHeight : e.Height;
            return new Rect(double.IsNaN(left) ? 0 : left, double.IsNaN(top) ? 0 : top, Math.Max(0, width), Math.Max(0, height));
        }



        public void DistributeHorizontally()
        {
            if (selectedElements.Count < 3)
                return;
            FrameworkElement leftAnchor = selectedElements.First();
            FrameworkElement rightAnchor = selectedElements.First();

            double leftExtent = Canvas.GetLeft(leftAnchor);
            double rightExtent = Canvas.GetLeft(rightAnchor) + rightAnchor.Width;

            double totalWidth = 0.0;
            foreach (FrameworkElement e in selectedElements)
            {
                totalWidth += e.Width;
                var left = Canvas.GetLeft(e);
                if (left < leftExtent)
                {
                    leftAnchor = e;
                    leftExtent = left;
                }
                var right = left + e.Width;
                if (right > rightExtent)
                {
                    rightAnchor = e;
                    rightExtent = right;
                }
            }
            double spacing = (rightExtent - leftExtent) - totalWidth;
            spacing = spacing / (selectedElements.Count - 1);

            leftExtent += leftAnchor.Width + spacing;
            foreach (FrameworkElement e in selectedElements)
            {
                if (e == leftAnchor || e == rightAnchor)
                    continue;
                SetLeft(e, leftExtent);
                leftExtent += e.Width + spacing;
            }
            IsModified = true;
        }



        public void DistributeVertically()
        {
            if (selectedElements.Count < 3)
                return;
            FrameworkElement topAnchor = selectedElements.First();
            FrameworkElement bottomAnchor = selectedElements.First();

            double topExtent = Canvas.GetTop(topAnchor);
            double bottomExtent = Canvas.GetTop(bottomAnchor) + bottomAnchor.Height;

            double totalHeight = 0.0;
            foreach (FrameworkElement e in selectedElements)
            {
                totalHeight += e.Height;
                var top = Canvas.GetTop(e);
                if (top < topExtent)
                {
                    topAnchor = e;
                    topExtent = top;
                }
                var bottom = top + e.Height;
                if (bottom > bottomExtent)
                {
                    bottomAnchor = e;
                    bottomExtent = bottom;
                }
            }
            double spacing = (bottomExtent - topExtent) - totalHeight;
            spacing = spacing / (selectedElements.Count - 1);

            topExtent += topAnchor.Height + spacing;
            foreach (FrameworkElement e in selectedElements)
            {
                if (e == topAnchor || e == bottomAnchor)
                    continue;
                SetTop(e, topExtent);
                topExtent += e.Height + spacing;
            }
            IsModified = true;
        }



        public void StackHorizontally()
        {
            if (selectedElements.Count < 2)
                return;

            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            var anchorLeft = Canvas.GetLeft(anchorElement);
            var anchorRight = anchorLeft + anchorElement.Width;
            var groupLeft = anchorLeft;
            var groupRight = anchorRight;
            foreach (FrameworkElement e in selectedElements)
            {
                if (e == anchorElement)
                    continue;
                var eLeft = Canvas.GetLeft(e);
                var eRight = eLeft + e.Width;
                var newLeft = eLeft;

                if (eLeft <= anchorLeft) // eLeft above anchorLeft
                {
                    if (eRight <= anchorRight) // aRight above anchorRight:
                    {
                        // move up
                        newLeft = groupLeft - e.Width;
                        groupLeft -= e.Width;
                    }
                    else // e straddles anchor: eLeft < anchorLeft && eRight > anchorRight
                    {
                        var leftDiff = anchorLeft - eLeft;
                        var botDiff = eRight - anchorRight;
                        if (leftDiff <= botDiff)
                        {
                            // move down
                            newLeft = groupRight;
                            groupRight += e.Width;
                        }
                        else
                        {
                            // move up
                            newLeft = groupLeft - e.Width;
                            groupLeft -= e.Width;
                        }
                    }
                }
                else // eLeft > anchorLeft: eLeft below anchorLeft
                {
                    if (eRight >= anchorRight)
                    {
                        // move down
                        newLeft = groupRight;
                        groupRight += e.Width;
                    }
                    else // eLeft > anchorLeft && eRight < anchorRight: e inside anchor
                    {
                        var leftDiff = eLeft - anchorLeft;
                        var botDiff = anchorRight - eRight;
                        if (leftDiff <= botDiff)
                        {
                            // move up
                            newLeft = groupLeft - e.Width;
                            groupLeft -= e.Width;
                        }
                        else
                        {
                            // move down
                            newLeft = groupRight;
                            groupRight += e.Width;
                        }
                    }
                }
                SetLeft(e, newLeft);
            }

            IsModified = true;
        }



        public void StackVertically()
        {
            if (selectedElements.Count < 2)
                return;

            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            var anchorTop = Canvas.GetTop(anchorElement);
            var anchorBottom = anchorTop + anchorElement.Height;
            var groupTop = anchorTop;
            var groupBottom = anchorBottom;
            foreach (FrameworkElement e in selectedElements)
            {
                if (e == anchorElement)
                    continue;
                var eTop = Canvas.GetTop(e);
                var eBottom = eTop + e.Height;
                var newTop = eTop;

                if (eTop <= anchorTop) // eTop above anchorTop
                {
                    if (eBottom <= anchorBottom) // aBottom above anchorBottom:
                    {
                        // move up
                        newTop = groupTop - e.Height;
                        groupTop -= e.Height;
                    }
                    else // e straddles anchor: eTop < anchorTop && eBottom > anchorBottom
                    {
                        var topDiff = anchorTop - eTop;
                        var botDiff = eBottom - anchorBottom;
                        if (topDiff <= botDiff)
                        {
                            // move down
                            newTop = groupBottom;
                            groupBottom += e.Height;
                        }
                        else
                        {
                            // move up
                            newTop = groupTop - e.Height;
                            groupTop -= e.Height;
                        }
                    }
                }
                else // eTop > anchorTop: eTop below anchorTop
                {
                    if (eBottom >= anchorBottom)
                    {
                        // move down
                        newTop = groupBottom;
                        groupBottom += e.Height;
                    }
                    else // eTop > anchorTop && eBottom < anchorBottom: e inside anchor
                    {
                        var topDiff = eTop - anchorTop;
                        var botDiff = anchorBottom - eBottom;
                        if (topDiff <= botDiff)
                        {
                            // move up
                            newTop = groupTop - e.Height;
                            groupTop -= e.Height;
                        }
                        else
                        {
                            // move down
                            newTop = groupBottom;
                            groupBottom += e.Height;
                        }
                    }
                }
                SetTop(e, newTop);
            }

            IsModified = true;
        }



        public void MoveSelectionRight()
        {
            ApplyToSelected(e =>
            {
                SetLeft(e, Canvas.GetLeft(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    SetLeft(e, Canvas.GetLeft(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionLeft()
        {
            ApplyToSelected(e =>
            {
                SetLeft(e, Canvas.GetLeft(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    SetLeft(e, Canvas.GetLeft(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionUp()
        {
            ApplyToSelected(e =>
            {
                SetTop(e, Canvas.GetTop(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    SetTop(e, Canvas.GetTop(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionDn()
        {
            ApplyToSelected(e =>
            {
                SetTop(e, Canvas.GetTop(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    SetTop(e, Canvas.GetTop(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        #endregion



        #region Sizing


        public void MakeSameWidth()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            IsModified = true;
            if (selectedElements.Count == 1)
            {
                SetWidth(anchorElement, CurrentCanvas.Width);
                return;
            }
            var w = anchorElement.Width;
            foreach (FrameworkElement e in selectedElements)
                SetWidth(e, w);
        }


        public void MakeSameHeight()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            IsModified = true;
            if (selectedElements.Count == 1)
            {
                SetHeight(anchorElement, CurrentCanvas.Height);
                return;
            }
            var h = anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
                SetHeight(e, h);
        }



        public void MakeSameSize()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;

            IsModified = true;
            if (selectedElements.Count == 1)
            {
                SetWidth(anchorElement, CurrentCanvas.Width);
                SetHeight(anchorElement, CurrentCanvas.Height);
                return;
            }
            var w = anchorElement.Width;
            var h = anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
            {
                SetWidth(e, w);
                SetHeight(e, h);
            }
        }



        public void ExchangeWidthAndHeight()
        {
            ApplyToSelected(e =>
            {
                var temp = e.Width;
                SetWidth(e, e.Height);
                SetHeight(e, temp);
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //{
            //    var temp = e.Width;
            //    SetWidth(e, e.Height);
            //    SetHeight(e, temp);
            //}
            //IsModified = true;
        }


        #endregion




        #region Z-Order

        public void ObjectSendBackward() => Reorder((index, count) => Math.Max(index - 1, 0));


        public void ObjectBringForward() => Reorder((index, count) => Math.Min(index + 1, count - 1));


        public void ObjectSendToBack() => Reorder((index, count) => 0);


        public void ObjectBringToFront() => Reorder((index, count) => count - 1);


        // Moves each selected element's node to the place among the nodes beside it that place gives, from its own place and their count
        private void Reorder(Func<int, int, int> place)
        {
            ApplyToSelected(e =>
            {
                var node = NodeOf(e);
                if (!(SiblingsOf(node) is System.Collections.ObjectModel.ObservableCollection<FlowElementBase> siblings))
                    return;
                int index = siblings.IndexOf(node);
                int target = place(index, siblings.Count);
                if (target != index)
                    siblings.Move(index, target);
            });
            OnElementsChanged();
        }


        #endregion



        #region DOM


        /// <summary>The DOM node an element on the page shows; null for one that shows none.</summary>
        private static FlowElementBase NodeOf(UIElement e) => FlowView.GetNode(e) as FlowElementBase;


        // Whether the panel holding an element places it: a stack panel places every child, a panel those with an alignment
        private static bool IsPlacedByItsPanel(UIElement e)
        {
            var parent = LogicalTreeHelper.GetParent(e);
            return parent is Stack || (parent is AlignPanel && AlignPanel.GetAlignment(e) != FlowAlignment.None);
        }


        // An element's position and size change in its node, which the view follows
        private static void SetLeft(UIElement e, double value)
        {
            if (IsPlacedByItsPanel(e))
                return;
            if (NodeOf(e) is FlowElementBase node)
                node.Left = value;
        }


        private static void SetTop(UIElement e, double value)
        {
            if (IsPlacedByItsPanel(e))
                return;
            if (NodeOf(e) is FlowElementBase node)
                node.Top = value;
        }


        private static void SetWidth(UIElement e, double value)
        {
            if (NodeOf(e) is FlowElementBase node)
                node.Width = value;
        }


        private static void SetHeight(UIElement e, double value)
        {
            if (NodeOf(e) is FlowElementBase node)
                node.Height = value;
        }


        /// <summary>The element that shows a node in a panel; null if there is none.</summary>
        private static FrameworkElement ViewOf(FlowBase node, Panel panel)
        {
            if (panel == null)
                return null;
            foreach (var element in panel.Children.OfType<FrameworkElement>())
            {
                if (FlowView.GetNode(element) == node)
                    return element;
                // The elements of groups, stack panels and panels, which can be selected too
                if ((element is Group || element is Stack || element is AlignPanel) && ViewOf(node, (Panel)element) is FrameworkElement inner)
                    return inner;
            }
            return null;
        }


        /// <summary>The nodes beside a node in the node holding it (its layer's elements, its group's children); null for none.</summary>
        private static IList<FlowElementBase> SiblingsOf(FlowElementBase node)
        {
            switch (node?.Parent)
            {
                case FlowLayer layer:
                    return layer.Elements;
                case FlowGroup group:
                    return group.Children;
                default:
                    return null;
            }
        }


        #endregion



        #region Generic Operations on Selected Elements


        public void ApplyToSelected(Action<FrameworkElement> action)
        {
            if (!selectedElements.Any())
                return;
            foreach (FrameworkElement e in selectedElements)
                action(e);
            IsModified = true;
        }


        #endregion




        #region Rubberbanding


        private void CreateRubberband()
        {
            if (Rubberband != null)
                return;
            Rubberband = new Rectangle();
            Rubberband.Stroke = new SolidColorBrush(Colors.LightGray);
            Rubberband.StrokeThickness = 2;
            Canvas.SetLeft(Rubberband, StartPoint.X);
            Canvas.SetTop(Rubberband, StartPoint.Y);
            CurrentCanvas.Children.Add(Rubberband);
        }


        private void RemoveRubberband()
        {
            if (Rubberband == null)
                return;
            CurrentCanvas.Children.Remove(Rubberband);
            Designer.ReleaseMouseCapture();
            Rubberband = null;
        }


        private void AdjustRubberband(Point currentPoint)
        {
            if (Rubberband == null)
                return;
            var delta = currentPoint - StartPoint;
            if (Math.Abs(delta.X) >= MIN_RUBBER_DELTA || Math.Abs(delta.Y) >= MIN_RUBBER_DELTA)
            {
                CreateRubberband();
                double width = Math.Abs(StartPoint.X - currentPoint.X);
                double height = Math.Abs(StartPoint.Y - currentPoint.Y);
                double left = Math.Min(StartPoint.X, currentPoint.X);
                double top = Math.Min(StartPoint.Y, currentPoint.Y);

                if (IsCtrlPressed)
                {
                    Rubberband.Width = Rubberband.Height = Math.Max(height, width);
                }
                else
                {
                    Rubberband.Width = width;
                    Rubberband.Height = height;
                }
                Canvas.SetLeft(Rubberband, left);
                Canvas.SetTop(Rubberband, top);
            }
        }

        #endregion



    }
}
