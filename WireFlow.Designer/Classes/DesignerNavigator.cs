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

            }
        }


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


        public bool IsModified {
            get { return Designer.IsModified; }
            set { Designer.IsModified = value; }
        }


        #endregion



        #region Computed Properties

        private Canvas CurrentCanvas => Designer.Canvas;

        public bool HasElementsSelected => selectedElements.Count > 0;
        public bool Has2PlusElementsSelected => selectedElements.Count >= 2;
        public bool Has3PlusElementsSelected => selectedElements.Count >= 3;
        public bool CanGroup => selectedElements.Count > 1;
        public bool CanUngroup => selectedElements.Count > 0;

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


        #endregion



        #region Construction & Initialization

        public DesignerNavigator(WireFlowEngine engine, string background)
        {
            Background = FindResource("DesignerBackgroundBrush") as Brush;
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
            Content = Designer;

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
                    if (!isModifierKeyPressed)
                        DeselectAll();
                    SelectElementOrItsGroup(clickedElement);
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
                    //Point position = Mouse.GetPosition(CurrentCanvas);
                    var deltaX = mousePos.X - StartPoint.X;
                    var deltaY = mousePos.Y - StartPoint.Y;

                    foreach (var selectedElement in selectedElements)
                    {
                        double oldTop = Canvas.GetTop(selectedElement);
                        double oldLeft = Canvas.GetLeft(selectedElement);

                        Canvas.SetTop(selectedElement, oldTop + deltaY);
                        Canvas.SetLeft(selectedElement, oldLeft + deltaX);
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

            IsDraggingShapes = false;
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
            CreateShape(SelectedDrawingTool);
            RemoveRubberband();
        }




        private void DesignerNavigateClickHandler(DesignerPanel designer, Point mousePos, FrameworkElement element)
        {
            var box = element as BaseDesignerObject;
            if (box == null)
                return;
            string currentPage = PageName;
            string href = box.Href;
            if (string.IsNullOrWhiteSpace(href))
                return;
            //TODO: Instead of navigating to page directly, ask the main window to do it, which in turn may call NavigateToPage, but then it may switch to an already open tab.
            OnNavigating(href);
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
                var pageToSave = Engine.SerializePage(CurrentPage as FlowPage, CurrentCanvas);
                Engine.SavePage(pageToSave);
                IsModified = false;
            });
        }



        public void SaveCurrentLayout()
        {
            if (CurrentPage == null || !IsModified)
                return;
            this.InvokeWithWaitCursor(() =>
            {
                Debug.WriteLine($"{this.GetType()}.{MethodBase.GetCurrentMethod().Name}: {CurrentPage?.Name} LAYOUT SAVED");
                var pageToSave = Engine.SerializePage(CurrentPage as FlowPage, CurrentCanvas);
                Engine.SaveLayout(pageToSave);
                IsModified = false;
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
            if (parent != CurrentLayer) // group
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


        void SelectElementOrItsGroup(FrameworkElement clickedElement)
        {
            if (clickedElement == null)
                return;
            var lparent = LogicalTreeHelper.GetParent(clickedElement);
            Canvas parent = lparent as Canvas;
            if (parent == null)
                return;

            if ((clickedElement is Part) || (parent is Layer) || (parent == CurrentCanvas)) // group, or freehand
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
            if (clickedElement.Parent != CurrentLayer)
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

                (adorner as ResizingAdorner).ResizeCompleted -= OnElementResized;
                aLayer?.Remove(adorner);
            }
            if (CurrentElement == clickedElement)
                CurrentElement = selectedElements.FirstOrDefault();
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
            adorner.ResizeCompleted += OnElementResized;
            adorner.IsClipEnabled = true;
            aLayer.Add(adorner);
        }



        private void OnElementResized(UIElement e, Size s)
        {
            IsModified = true;
        }



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
                    ResetAppModes();
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



        void CreateShape(IDrawingTool drawingTool)
        {
            if (drawingTool == null)
                return;

            //SelectDefaultLayer(); DO NOT!
            if (CurrentLayer == null)
                return;

            if (Rubberband == null)
                return;

            var element = drawingTool.CreateElement();
            if (element == null)
                return;

            CurrentLayer.Children.Add(element);

            element.Width = Rubberband.Width;
            element.Height = Rubberband.Height;
            Canvas.SetLeft(element, Canvas.GetLeft(Rubberband));
            Canvas.SetTop(element, Canvas.GetTop(Rubberband));

            SelectElement(element);

            IsModified = true;
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
        }




        public void DuplicateSelectedElements()
        {
            if (selectedElements.Count == 0)
                return;

            var newElements = new List<FrameworkElement>();
            foreach (var e in selectedElements)
            {
                var lparent = LogicalTreeHelper.GetParent(e);
                Panel parent = lparent as Panel;
                if (parent == null)
                    continue;
                string xaml = XamlWriter.Save(e);
                StringReader stringReader = new StringReader(xaml);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                FrameworkElement newElement = (FrameworkElement)XamlReader.Load(xmlReader);
                parent.Children.Add(newElement);
                newElements.Add(newElement);
            }
            DeselectAll();
            selectedElements.AddRange(newElements);
            RebuildSelection();

            IsModified = true;
        }


        public void DeleteSelectedElements()
        {
            if (selectedElements.Count == 0)
                return;
            foreach (UIElement e in selectedElements)
            {
                //var vparent = VisualTreeHelper.GetParent(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Panel parent = lparent as Panel;
                if (parent != null)
                    parent.Children.Remove(e);
                //DesignerCanvas.Children.Remove(e);
            }
            selectedElements.Clear();
            CurrentElement = CurrentCanvas;

            IsModified = true;
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

            var anchorElement = selectedElements[0];
            var anchorParent = LogicalTreeHelper.GetParent(anchorElement) as Layer;
            if (anchorParent == null)
                return;
            //var anchorIndex = anchorParent.Children.IndexOf(anchorElement);


            Group group = new Group();
            //group.Background = Brushes.Transparent;

            Point pos = new Point();
            var bounds = GetGroupBounds(selectedElements);

            // Reparent and reposition the elements
            foreach (FrameworkElement e in selectedElements)
            {
                pos.X = Canvas.GetLeft(e);
                pos.Y = Canvas.GetTop(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Panel parent = lparent as Panel;
                if (parent != null)
                {
                    parent.Children.Remove(e);
                    group.Children.Add(e);
                    Canvas.SetLeft(e, pos.X - bounds.X);
                    Canvas.SetTop(e, pos.Y - bounds.Y);
                }
            }
            group.Width = bounds.Width;
            group.Height = bounds.Height;
            Canvas.SetLeft(group, bounds.Left);
            Canvas.SetTop(group, bounds.Top);

            anchorParent.Children.Add(group);

            DeselectAll();
            SelectElement(group);

            IsModified = true;
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
            List<FrameworkElement> list = new List<FrameworkElement>();

            foreach (FrameworkElement e in selectedElements)
            {
                Canvas c = e as Canvas;
                if (c == null)
                {
                    list.Add(e);
                    continue;
                }
                UngroupElement(c, list);
            }
            DeselectAll();
            foreach (FrameworkElement e in list)
            {
                SelectElement(e);
            }

            IsModified = true;
        }


        public void UngroupElement(Canvas group, List<FrameworkElement> newSelection)
        {
            // Remember the layer
            var groupLayer = LogicalTreeHelper.GetParent(group) as Layer;
            if (groupLayer == null)
                return;
            var groupIndex = groupLayer.Children.IndexOf(group);
            // Collect the elements of the group
            List<FrameworkElement> list = group.Children.Cast<FrameworkElement>().ToList();
            Point groupPos = new Point()
            {
                X = Canvas.GetLeft(group),
                Y = Canvas.GetTop(group)
            };
            // Delete the group-canvas from its parent
            groupLayer.Children.Remove(group);
            // Reparent all elements to the Layer
            foreach (FrameworkElement element in list)
            {
                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);
                x += groupPos.X;
                y += groupPos.Y;
                group.Children.Remove(element);


                groupLayer.Children.Insert(groupIndex, element);
                groupIndex++;

                Canvas.SetLeft(element, x);
                Canvas.SetTop(element, y);
            }
            // Select elements of teh original group
            newSelection.AddRange(list);

            IsModified = true;
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
                Canvas.SetTop(anchorElement, 0);
                return;
            }
            var top = Canvas.GetTop(anchorElement);
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetTop(e, top);
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
                Canvas.SetTop(anchorElement, CurrentCanvas.Height - anchorElement.Height);
                return;
            }
            var bottom = Canvas.GetTop(anchorElement) + anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetTop(e, bottom - e.Height);
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
                Canvas.SetLeft(anchorElement, 0);
                return;
            }
            var left = Canvas.GetLeft(anchorElement);
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetLeft(e, left);
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
                Canvas.SetLeft(anchorElement, CurrentCanvas.Width - anchorElement.Width);
                return;
            }
            var right = Canvas.GetLeft(anchorElement) + anchorElement.Width;
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetLeft(e, right - e.Width);
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
                Canvas.SetLeft(anchorElement, hCenter - anchorElement.Width / 2);
                return;
            }
            var center = Canvas.GetLeft(anchorElement) + anchorElement.Width / 2;
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetLeft(e, center - (e.Width / 2));
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
                Canvas.SetTop(anchorElement, vCenter - anchorElement.Height / 2);
                return;
            }
            var center = Canvas.GetTop(anchorElement) + (anchorElement.Height / 2);
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetTop(e, center - (e.Height / 2));
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
                Canvas.SetLeft(anchorElement, hCenter - anchorElement.Width / 2);
                Canvas.SetTop(anchorElement, vCenter - anchorElement.Height / 2);
                return;
            }
            foreach (FrameworkElement e in selectedElements)
            {
                Canvas.SetTop(e, vCenter - (e.Height / 2));
                Canvas.SetLeft(e, hCenter - (e.Width / 2));
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
                Canvas.SetLeft(e, leftExtent);
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
                Canvas.SetTop(e, topExtent);
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
                Canvas.SetLeft(e, newLeft);
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
                Canvas.SetTop(e, newTop);
            }

            IsModified = true;
        }



        public void MoveSelectionRight()
        {
            ApplyToSelected(e =>
            {
                Canvas.SetLeft(e, Canvas.GetLeft(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    Canvas.SetLeft(e, Canvas.GetLeft(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionLeft()
        {
            ApplyToSelected(e =>
            {
                Canvas.SetLeft(e, Canvas.GetLeft(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    Canvas.SetLeft(e, Canvas.GetLeft(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionUp()
        {
            ApplyToSelected(e =>
            {
                Canvas.SetTop(e, Canvas.GetTop(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    Canvas.SetTop(e, Canvas.GetTop(e) - (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            //IsModified = true;
        }


        public void MoveSelectionDn()
        {
            ApplyToSelected(e =>
            {
                Canvas.SetTop(e, Canvas.GetTop(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //    Canvas.SetTop(e, Canvas.GetTop(e) + (IsShiftPressed ? 10 : IsCtrlPressed ? 5 : 1));
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
                anchorElement.Width = CurrentCanvas.Width;
                return;
            }
            var w = anchorElement.Width;
            foreach (FrameworkElement e in selectedElements)
                e.Width = w;
        }


        public void MakeSameHeight()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;
            IsModified = true;
            if (selectedElements.Count == 1)
            {
                anchorElement.Height = CurrentCanvas.Height;
                return;
            }
            var h = anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
                e.Height = h;
        }



        public void MakeSameSize()
        {
            var anchorElement = selectedElements.FirstOrDefault();
            if (anchorElement == null)
                return;

            IsModified = true;
            if (selectedElements.Count == 1)
            {
                anchorElement.Width = CurrentCanvas.Width;
                anchorElement.Height = CurrentCanvas.Height;
                return;
            }
            var w = anchorElement.Width;
            var h = anchorElement.Height;
            foreach (FrameworkElement e in selectedElements)
            {
                e.Width = w;
                e.Height = h;
            }
        }



        public void ExchangeWidthAndHeight()
        {
            ApplyToSelected(e =>
            {
                var temp = e.Width;
                e.Width = e.Height;
                e.Height = temp;
            });
            //if (!selectedElements.Any())
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //{
            //    var temp = e.Width;
            //    e.Width = e.Height;
            //    e.Height = temp;
            //}
            //IsModified = true;
        }


        #endregion




        #region Z-Order

        public void ObjectSendBackward()
        {
            ApplyToSelected(e =>
            {
                //var vparent = VisualTreeHelper.GetParent(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Canvas parent = lparent as Canvas;
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(e);
                    if (i > 0)
                    {
                        parent.Children.RemoveAt(i);
                        parent.Children.Insert(i - 1, e);
                    }
                }
            });
            //if (!HasElementsSelected)
            //    return;
            //foreach (UIElement e in selectedElements)
            //{
            //    //var vparent = VisualTreeHelper.GetParent(e);
            //    var lparent = LogicalTreeHelper.GetParent(e);
            //    Canvas parent = lparent as Canvas;
            //    if (parent != null)
            //    {
            //        int i = parent.Children.IndexOf(e);
            //        if (i > 0)
            //        {
            //            parent.Children.RemoveAt(i);
            //            parent.Children.Insert(i - 1, e);
            //        }
            //    }
            //}
            //IsModified = true;
        }


        public void ObjectBringForward()
        {
            ApplyToSelected(e =>
            {
                //var vparent = VisualTreeHelper.GetParent(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Canvas parent = lparent as Canvas;
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(e);
                    if (i < parent.Children.Count - 1)
                    {
                        parent.Children.RemoveAt(i);
                        parent.Children.Insert(i + 1, e);
                    }
                }
            });
            //if (!HasElementsSelected)
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //{
            //    //var vparent = VisualTreeHelper.GetParent(e);
            //    var lparent = LogicalTreeHelper.GetParent(e);
            //    Canvas parent = lparent as Canvas;
            //    if (parent != null)
            //    {
            //        int i = parent.Children.IndexOf(e);
            //        if (i < parent.Children.Count - 1)
            //        {
            //            parent.Children.RemoveAt(i);
            //            parent.Children.Insert(i + 1, e);
            //        }
            //    }
            //}
            //IsModified = true;
        }


        public void ObjectSendToBack()
        {
            ApplyToSelected(e =>
            {
                //var vparent = VisualTreeHelper.GetParent(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Canvas parent = lparent as Canvas;
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(e);
                    parent.Children.RemoveAt(i);
                    parent.Children.Insert(0, e);
                }
            });
            //if (!HasElementsSelected)
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //{
            //    //var vparent = VisualTreeHelper.GetParent(e);
            //    var lparent = LogicalTreeHelper.GetParent(e);
            //    Canvas parent = lparent as Canvas;
            //    if (parent != null)
            //    {
            //        int i = parent.Children.IndexOf(e);
            //        parent.Children.RemoveAt(i);
            //        parent.Children.Insert(0, e);
            //    }
            //}
            //IsModified = true;
        }


        public void ObjectBringToFront()
        {
            ApplyToSelected( e =>
            {
                //var vparent = VisualTreeHelper.GetParent(e);
                var lparent = LogicalTreeHelper.GetParent(e);
                Canvas parent = lparent as Canvas;
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(e);
                    parent.Children.RemoveAt(i);
                    parent.Children.Insert(parent.Children.Count, e);
                }

            });
            //if (!HasElementsSelected)
            //    return;
            //foreach (FrameworkElement e in selectedElements)
            //{
            //    //var vparent = VisualTreeHelper.GetParent(e);
            //    var lparent = LogicalTreeHelper.GetParent(e);
            //    Canvas parent = lparent as Canvas;
            //    if (parent != null)
            //    {
            //        int i = parent.Children.IndexOf(e);
            //        parent.Children.RemoveAt(i);
            //        parent.Children.Insert(parent.Children.Count, e);
            //    }
            //}
            //IsModified = true;
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
