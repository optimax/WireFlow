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


        // A tab hanging from the top edge of the area, at its center, with its lower corners rounded; clicks go through it to
        // the page under it
        private Border CreateNavigateBadge()
        {
            var text = new TextBlock { Text = "Navigate Mode", FontWeight = FontWeights.SemiBold, FontSize = 12 };
            text.SetResourceReference(TextBlock.ForegroundProperty, "NavigateBadgeTextBrush");
            navigateBadge = new Border
            {
                Child = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(16, 3, 16, 4),
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed,
            };
            navigateBadge.SetResourceReference(Border.BackgroundProperty, "NavigateBadgeBrush");
            return navigateBadge;
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
                BeginGestureStep();
        }



        public void DesignerSelectMoveHandler(DesignerPanel designer, Point mousePos)
        {
            Point currentPoint = mousePos;

            if (IsDraggingShapes)
            {
                bool moveElements =
                    ((Math.Abs(mousePos.X - StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(mousePos.Y - StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance));

                Debug.WriteLine($"MOUSE MOVE: {moveElements} {mousePos}:{StartPoint}  {SystemParameters.MinimumVerticalDragDistance}");

                if (moveElements)
                {
                    pendingChildSelection = null;
                    //Point position = Mouse.GetPosition(CurrentCanvas);
                    var deltaX = mousePos.X - StartPoint.X;
                    var deltaY = mousePos.Y - StartPoint.Y;

                    foreach (var selectedElement in selectedElements)
                    {
                        double oldTop = Canvas.GetTop(selectedElement);
                        double oldLeft = Canvas.GetLeft(selectedElement);

                        SetTop(selectedElement, oldTop + deltaY);
                        SetLeft(selectedElement, oldLeft + deltaX);
                    }
                    StartPoint = mousePos;
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
            EndGestureStep();
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
            // Tab name change
            PageName = href;
            NavigatorDocument.Title = href;


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
            // Tab name change
            PageName = href;
            NavigatorDocument.Title = href;


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


        // The selected group or stack panel holding the element, if any
        private FrameworkElement SelectedContainerOf(FrameworkElement element)
        {
            if (element == null)
                return null;
            for (var parent = LogicalTreeHelper.GetParent(element); parent is Group || parent is Stack; parent = LogicalTreeHelper.GetParent(parent))
                if (selectedElements.Contains(parent))
                    return (FrameworkElement)parent;
            return null;
        }


        /// <summary>Selects the groups and stack panels holding the selected elements, as Esc does; with none, deselects all, which leaves the page selected.</summary>
        public void SelectContainers()
        {
            var containers = selectedElements.Select(e => LogicalTreeHelper.GetParent(e)).OfType<FrameworkElement>().Where(p => p is Group || p is Stack).Distinct().ToList();
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

            if ((clickedElement is Part) || (parent is Layer) || (parent is Stack) || (parent == CurrentCanvas)) // not in a group, or freehand
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
            adorner.IsClipEnabled = true;
            aLayer.Add(adorner);
        }



        private void OnElementResizeStarted(UIElement e)
        {
            BeginGestureStep();
        }


        private void OnElementResized(UIElement e, Size s)
        {
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
                history.Detach();
            }
            gestureStepOpen = false;
            history = page == null ? null : new FlowHistory(page);
            idsOutside = null;
            if (history != null)
            {
                history.Changed += HistoryChanged;
                history.NodeAdded += GiveIds;
            }
            OnHistoryStateChanged();
        }


        // The IDs of the project's other documents, read once the page needs them
        private HashSet<string> idsOutside;


        private HashSet<string> IdsOutside => idsOutside ??= Engine.IdsOutside(CurrentPage?.Name, CurrentPage is FlowPart);


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
            return IdsOutside.Contains(id) || Engine.IsIdGivenElsewhere(id, CurrentPage?.Name);
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
                taken.UnionWith(Engine.IdsGivenElsewhere(CurrentPage?.Name));
                taken.UnionWith(FlowIds.IdsIn(CurrentPage));
                node.Id = FlowIds.ForKind(node.Type, taken);
                Engine.NoteIdGiven(node.Id, CurrentPage?.Name);
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
            // An element drawn starting inside a stack panel becomes its last child, which the panel places
            Stack stack = FindStackAt(CurrentLayer, start);
            if (stack != null && FlowView.GetNode(stack) is FlowStack stackNode)
            {
                node.Left = 0;
                node.Top = 0;
                stackNode.Children.Add(node);
                var drawn = ViewOf(node, stack);
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
        /// The innermost stack panel at a point of a layer or of a panel (in its coordinates), the topmost where panels
        /// overlap; null if there is none.
        /// </summary>
        private static Stack FindStackAt(Panel container, Point point)
        {
            foreach (var stack in container.Children.OfType<Stack>().Reverse())
            {
                var bounds = new Rect(Canvas.GetLeft(stack), Canvas.GetTop(stack), stack.ActualWidth, stack.ActualHeight);
                if (bounds.Contains(point))
                    return FindStackAt(stack, new Point(point.X - bounds.X, point.Y - bounds.Y)) ?? stack;
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
                // Groups and stack panels; a part reference keeps its part
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
        /// Puts a group's or stack panel's children in its place in its layer, where they are on the page (as drawn, which
        /// is where a stack panel put them), and takes it away.
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


        // An element's position and size change in its node, which the view follows
        private static void SetLeft(UIElement e, double value)
        {
            if (LogicalTreeHelper.GetParent(e) is Stack)
                return;
            if (NodeOf(e) is FlowElementBase node)
                node.Left = value;
        }


        private static void SetTop(UIElement e, double value)
        {
            if (LogicalTreeHelper.GetParent(e) is Stack)
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
                // The elements of groups and stack panels, which can be selected too
                if ((element is Group || element is Stack) && ViewOf(node, (Panel)element) is FrameworkElement inner)
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
