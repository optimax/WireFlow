using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WireFlow
{
    public class WireFlowEngine: INotifyPropertyChanged, IFlowRepository, ILayoutListProvider
    {

        #region Fields

        private int layerNo = 0;

        private bool showCenter;
        private bool showGrid;
        private bool showThirds;
        private bool showGoldenRatios;
        private bool show4x4Grid;
        private bool show5x5Grid;
        private bool show7x7Grid;
        private bool show12x12Grid;

        #endregion


        #region Properties

        //public static CommandBinding NavigateCommandBinding { get; set; }


        public FlowProject Project { get; set; }

        public string DefaultFileExt => Repository.DefaultFileExt;
        public string DefaultIndexNode => Repository.DefaultIndexNode;
        public string ProjectFileName => Repository.ProjectFileName;

        public IList<string> Layouts => Repository.Layouts;
        public IList<string> Pages => Repository.Pages;
        public IList<string> Parts => Repository.Parts;
        public IList<string> Images => Repository.Images;






        //  Events

        public Action<string> NavigateToNodeFunc { get; set; }
        public Action<string> ProjectFileLoaded { get; set; }






        public string DocumentFileFilters => Repository.DocumentFileFilters;


        private IFlowRepository Repository { get; set; }
        public RoutedUICommand NavigateToCommand { get; }




        public bool ShowCenter
        {
            get { return showCenter; }
            set
            {
                if (showCenter != value)
                {
                    showCenter = value;
                    OnPropertyChanged(nameof(ShowCenter));
                }
            }
        }

        public bool ShowGrid
        {
            get { return showGrid; }
            set
            {
                if (showGrid != value)
                {
                    showGrid = value;
                    OnPropertyChanged(nameof(ShowGrid));
                }
            }
        }

        public bool ShowThirds
        {
            get { return showThirds; }
            set
            {
                if (showThirds != value)
                {
                    showThirds = value;
                    OnPropertyChanged(nameof(ShowThirds));
                }
            }
        }


        public bool ShowGoldenRatios
        {
            get { return showGoldenRatios; }
            set
            {
                if (showGoldenRatios != value)
                {
                    showGoldenRatios = value;
                    OnPropertyChanged(nameof(ShowGoldenRatios));
                }
            }
        }


        public bool Show4x4Grid
        {
            get { return show4x4Grid; }
            set
            {
                if (show4x4Grid != value)
                {
                    show4x4Grid = value;
                    OnPropertyChanged(nameof(Show4x4Grid));
                }
            }
        }

        public bool Show5x5Grid
        {
            get { return show5x5Grid; }
            set
            {
                if (show5x5Grid != value)
                {
                    show5x5Grid = value;
                    OnPropertyChanged(nameof(Show5x5Grid));
                }
            }
        }


        public bool Show7x7Grid
        {
            get { return show7x7Grid; }
            set
            {
                if (show7x7Grid != value)
                {
                    show7x7Grid = value;
                    OnPropertyChanged(nameof(Show7x7Grid));
                }
            }
        }



        public bool Show12x12Grid
        {
            get { return show12x12Grid; }
            set
            {
                if (show12x12Grid != value)
                {
                    show12x12Grid = value;
                    OnPropertyChanged(nameof(Show12x12Grid));
                }
            }
        }


        #endregion



        #region Repository Operations

        public FlowProject NewProject(string docName)
        {
            Project = Repository.NewProject(docName);
            return Project;
        }


        public void CloseProject()
        {
            Project = null;
        }

        public FlowProject OpenProject(string projectFileName)
        {
            Project = Repository.OpenProject(projectFileName);
            return Project;
        }

        public FlowPage GetMasterPage(string nodeName)
        {
            return Repository.GetMasterPage(nodeName);
        }

        public FlowPage GetPage(string nodeName)
        {
            return Repository.GetPage(nodeName);
        }

        public FlowElementBase GetPart(string partName)
        {
            return Repository.GetPart(partName);
        }

        public byte[] LoadImage(string imageName)
        {
            return Repository.LoadImage(imageName);
        }


        public void CreatePage(string pageName, string layoutName)
        {
            Repository.CreatePage(pageName, layoutName);
        }


        public void DeletePage(string pageName)
        {
            Repository.DeletePage(pageName);
        }


        public void SavePage(FlowPage page)
        {
            Repository.SavePage(page);
        }



        public void CreateLayout(string pageName, string layoutName)
        {
            Repository.CreateLayout(pageName, layoutName);
        }


        public void DeleteLayout(string pageName)
        {
            Repository.DeleteLayout(pageName);
        }

        public void SaveLayout(FlowPage page)
        {
            Repository.SaveLayout(page);
        }

        public void RenameLayout(string oldName, string newName)
        {
           Repository.RenameLayout(oldName, newName);
        }



        public void CreatePart(string partName)
        {
            Repository.CreatePart(partName);
        }


        public void DeletePart(string partName)
        {
            Repository.DeletePart(partName);
        }

        public void SavePart(FlowPart part)
        {
            Repository.SavePart(part);
        }

        public void RenamePart(string oldName, string newName)
        {
            Repository.RenamePart(oldName, newName);
        }



        public void SaveProject()
        {
            Repository.SaveProject();
        }


        public void RenamePage(string oldName, string newName)
        {
            Repository.RenamePage(oldName, newName);
        }



        public ImportImageResult CanImportImage(string fileName)
        {
            return Repository.CanImportImage(fileName);
        }


        public ImportImageResult ImportImage(string fileName)
        {
            return Repository.ImportImage(fileName);
        }



        public void ImportImages(IEnumerable<string> imageNames)
        {
            Repository.ImportImages(imageNames);
        }


        public void DeleteImages(IList imageNames)
        {
            Repository.DeleteImages(imageNames);
        }


        #endregion



        #region Methods


        public static Layout UserLayout(Canvas canvas)
        {
            var layout = canvas.Children[canvas.Children.Count - 1] as Layout;
            return layout;
        }


        public WireFlowEngine(IFlowRepository repository)
        {
            Repository = repository;
            Repository.ProjectFileLoaded = s =>
            {
                this.ProjectFileLoaded?.Invoke(s);
            };
            NavigateToCommand = new RoutedUICommand("", "NavigateTo", this.GetType());
            //WireFlowEngine.NavigateCommandBinding = new CommandBinding(NavigateToCommand, (s,e) => NavigateToNode(e.Parameter as FlowElementBase));
        }


        ////TODO: Unused - delete
        //private void NavigateToNode(FlowElementBase link)
        //{
        //    //Debug.WriteLine($"{this.GetType()} Navigate to node ---> {link.Href}");
        //    NavigateToNodeFunc?.Invoke(link.Href);
        //}


        #endregion



        #region Serialization (Un-Rendering)



        public FlowPage SerializePage(FlowPage page, Canvas canvas)
        {
            if (canvas.Children.Count == 0)
                return null;
            Debug.WriteLine("CALLED: "+MethodBase.GetCurrentMethod());

            // The size given to the page in the Designer is the page's own when it has one or the size differs from the one
            // it inherits; a page without one keeps following its layout or the project
            var inherited = InheritedPageSize(page);
            page.Width = !double.IsNaN(page.Width) || canvas.Width != inherited.Width ? canvas.Width : double.NaN;
            page.Height = !double.IsNaN(page.Height) || canvas.Height != inherited.Height ? canvas.Height : double.NaN;
            // Its background likewise: its own when it has one or it differs from the one it inherits; none (cleared in the
            // property grid) makes it follow its layout or the project again
            string background = (canvas.Background as SolidColorBrush)?.Color.ToString();
            string inheritedBackground = BackgroundBrush(InheritedPageBackground(page)).Color.ToString();
            page.Background = background != null && (page.Background != null || background != inheritedBackground) ? background : null;

            var layout = UserLayout(canvas);

            return SerializeLayout(page, layout);
        }


        /// <summary>
        /// The size a page or layout without one of its own is shown at, as RenderPage gives it: the size of its nearest
        /// layout that has one, else the project's.
        /// </summary>
        public Size InheritedPageSize(FlowPage page)
        {
            double width = double.NaN, height = double.NaN;
            var master = Repository.GetMasterPage(page?.Master);
            // A layout that names itself or an earlier one as its layout ends the search
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { page?.Name ?? "" };
            while (master != null && seen.Add(master.Name ?? "") && (double.IsNaN(width) || double.IsNaN(height)))
            {
                if (double.IsNaN(width))
                    width = master.Width;
                if (double.IsNaN(height))
                    height = master.Height;
                master = Repository.GetMasterPage(master.Master);
            }
            return new Size(double.IsNaN(width) ? Project.Width : width, double.IsNaN(height) ? Project.Height : height);
        }



        /// <summary>
        /// The background a page or layout without one of its own is shown in, as RenderPage gives it: that of its nearest
        /// layout that has one, else the project's.
        /// </summary>
        public string InheritedPageBackground(FlowPage page)
        {
            var master = Repository.GetMasterPage(page?.Master);
            // A layout that names itself or an earlier one as its layout ends the search
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { page?.Name ?? "" };
            while (master != null && seen.Add(master.Name ?? ""))
            {
                if (!string.IsNullOrWhiteSpace(master.Background))
                    return master.Background;
                master = Repository.GetMasterPage(master.Master);
            }
            return Project.Background;
        }


        // A page's background color as a brush: the default background for none, or for one that is not a color
        private static SolidColorBrush BackgroundBrush(string color)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(color))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Constants.DEFAULT_BACKGROUND));
        }



        public FlowPage SerializeLayout(FlowPage page, Layout layout)
        {
            if (layout == null)
                return null;
            page.Layers.Clear();
            foreach (var layer in layout.Children.Cast<Layer>())
            {
                if (layer == null)
                {
                    Debug.WriteLine($"NON-LAYER Ignored in SerializeLayout");
                    continue;
                }

                if (layer is Master)
                {
                    Debug.WriteLine($"MASTER: {layer.Name} skipped.");
                    var masterName = layer.PageNode.Name;
                }
                else
                {
                    Debug.WriteLine($"LAYER: {layer.Name} saved.");
                    FlowLayer dataLayer = LayerElementToFlowLayer(layer);
                    page.Layers.Add(dataLayer);
                }
            }
            return page;
        }



        public FlowPart SerializePart(FlowPart page, Canvas canvas)
        {
            //TODO
            return null;
        }


        public FlowLayer LayerElementToFlowLayer(Layer uiLayer)
        {
            var result = new FlowLayer()
            {
                Name = uiLayer.Name
                //TODO: Hidden and locked
            };

            foreach (var uiElement in uiLayer.Children.Cast<UIElement>())
            {
                FlowElementBase element = ConvertUiElement(uiElement);
                if (element != null)
                    result.Elements.Add(element);
            }



            return result;
        }



        /// <summary>
        /// The DOM node of an element made as a WPF control, as a drawing tool makes a new one with its defaults.
        /// </summary>
        public FlowElementBase ToNode(UIElement uiElement) => ConvertUiElement(uiElement);


        private FlowElementBase ConvertUiElement(UIElement uiElement)
        {
            FlowElementBase element;
            string uiElementType = uiElement.GetType().Name.ToLower();
            Debug.WriteLine($"ConvertUIElement: {uiElementType}");
            switch (uiElementType)
            {
                case "box":
                    element = ConvertBox(uiElement);
                    break;
                case "imagebox":
                    element = ConvertImageBox(uiElement);
                    break;
                case "wireflowrectangle":
                    element = ConvertRectangle(uiElement);
                    break;
                case "wireflowellipse":
                    element = ConvertEllipse(uiElement);
                    break;
                case "wireflowtext":
                    element = ConvertTextBlock(uiElement);
                    break;
                case "group":
                    element = ConvertGroup(uiElement);
                    break;
                case "stack":
                    element = ConvertStack(uiElement);
                    break;
                case "part":
                    element = ConvertPart(uiElement);
                    break;
                default:
                    element = ConvertUnknown(uiElement);
                    break;
            }
            return element;
        }



        FlowElementBase ConvertGroup(UIElement element)
        {
            Group group = element as Group;
            if (group == null)
                return null;
            var result = new FlowGroup()
            {
                Type = "group",
                Name = string.IsNullOrWhiteSpace(group.Name) ? null : group.Name,
                Left = group.Left,
                Top = group.Top,
                Height = group.Height,
                Width = group.Width,
                Transparency = 1 - group.Opacity,
                Href = group.Href,
            };
            foreach (var uiChild in group.Children.Cast<UIElement>())
            {
                FlowElementBase dataElement = ConvertUiElement(uiChild);
                if (dataElement != null)
                    result.Children.Add(dataElement);
            }
            return result;
        }


        FlowElementBase ConvertStack(UIElement element)
        {
            if (!(element is Stack stack))
                return null;
            var result = new FlowStack()
            {
                Type = "stack",
                Name = string.IsNullOrWhiteSpace(stack.Name) ? null : stack.Name,
                Left = stack.Left,
                Top = stack.Top,
                Height = stack.Height,
                Width = stack.Width,
                Direction = stack.Direction.ToFlow(),
                Fill = stack.Background?.ToString(),
                Transparency = 1 - stack.Opacity,
                Href = stack.Href,
            };
            foreach (var uiChild in stack.Children.Cast<UIElement>())
            {
                FlowElementBase dataElement = ConvertUiElement(uiChild);
                if (dataElement != null)
                    result.Children.Add(dataElement);
            }
            return result;
        }


        FlowElementBase ConvertPart(UIElement element)
        {
            Part part = element as Part;
            if (part == null)
                return null;
            var result = new FlowPart()
            {
                Type = "part",
                Name = string.IsNullOrWhiteSpace(part.TemplateName) ? null : part.TemplateName,
                Left = part.Left,
                Top = part.Top,
                Height = part.Height,
                Width = part.Width,
                Transparency = 1 - part.Opacity,
                Href = part.Href,
            };
            return result;
        }


        FlowElementBase ConvertImageBox(UIElement element)
        {
            ImageBox img = element as ImageBox;
            if (img == null)
                return null;
            var result = new FlowImage()
            {
                Type = "image",
                Name = string.IsNullOrWhiteSpace(img.Name)?null:img.Name,
                Image = img.ImageFileName,
                Stretch = img.Stretch.ToFlow(),
                Left = img.Left,
                Top = img.Top,
                Height = img.Height,
                Width = img.Width,
                Transparency = 1 - img.Opacity,
                Href = img.Href,
            };
            return result;
        }


        FlowElement ConvertBox(UIElement element)
        {
            Box box = element as Box;
            if (box == null)
                return null;
            var result = new FlowElement()
            {
                Type = "box",
                Name = string.IsNullOrWhiteSpace(box.Name) ? null : box.Name,
                Left = box.Left,
                Top = box.Top,
                Height = box.Height,
                Width = box.Width,
                Color = box.Foreground.ToString(),
                Fill = box.Fill.ToString(),
                Font = box.FontFamily.ToString(),
                FontSize = box.FontSize,
                FontWeight = box.FontWeight.ToOpenTypeWeight(),
                FontStretch = box.FontStretch.ToFlow(),
                HorizontalAlignment = box.TextHorizontalAlignment.ToFlow(),
                VerticalAlignment = box.TextVerticalAlignment.ToFlow(),
                LineHeight = box.LineHeight,
                RadiusX = box.BorderRadiusX,
                RadiusY = box.BorderRadiusY,
                Stroke = box.BorderColor.ToString(),
                StrokeThickness = box.BorderWidth,
                Text = box.Text,
                TextAlignment = box.TextAlignment.ToFlow(),
                TextWrapping = box.TextWrapping.ToFlow(),
                Transparency = 1 - box.Opacity,
                Href = box.Href,
                //Href
                //Children
                //Image
                //Stretch
            };
            return result;
        }


        FlowElement ConvertTextBlock(UIElement element)
        {
            WireFlowText text = element as WireFlowText;
            if (text == null)
                return null;
            var result = new FlowElement()
            {
                Type = "text",
                Name = string.IsNullOrWhiteSpace(text.Name) ? null : text.Name,
                Left = Canvas.GetLeft(text),
                Top = Canvas.GetTop(text),
                Height = text.Height,
                Width = text.Width,
                Color = text.Foreground?.ToString(),
                Fill = text.Background?.ToString(),
                Font = text.FontFamily?.ToString(),
                FontSize = text.FontSize,
                FontStretch = text.FontStretch.ToFlow(),
                FontWeight = text.FontWeight.ToOpenTypeWeight(),
                HorizontalAlignment = text.HorizontalAlignment.ToFlow(),
                VerticalAlignment = text.TextVerticalAlignment.ToFlow(),
                LineHeight = text.LineHeight,
                // A text's padding is saved as its StrokeThickness, which ApplyText reads
                StrokeThickness = text.TextPadding.Left,
                Text = text.Text,
                TextAlignment = text.TextAlignment.ToFlow(),
                TextWrapping = text.TextWrapping.ToFlow(),
                Transparency = 1 - text.Opacity,
                Href = text.Href,
            };
            return result;
        }





        FlowElement ConvertRectangle(UIElement element)
        {
            WireFlowRectangle box = element as WireFlowRectangle;
            if (box == null)
                return null;
            var result = new FlowElement()
            {
                Type = "rect",
                Name = string.IsNullOrWhiteSpace(box.Name) ? null : box.Name,
                Left = Canvas.GetLeft(box),
                Top = Canvas.GetTop(box),
                Height = box.Height,
                Width = box.Width,
                Fill = box.Fill?.ToString(),
                HorizontalAlignment = box.HorizontalAlignment.ToFlow(),
                VerticalAlignment = box.VerticalAlignment.ToFlow(),
                RadiusX = box.RadiusX,
                RadiusY = box.RadiusY,
                Stroke = box.Stroke?.ToString(),
                StrokeThickness = box.StrokeThickness,
                Transparency = 1 - box.Opacity,
                Href = box.Href,
            };
            return result;
        }



        FlowElement ConvertEllipse(UIElement element)
        {
            WireFlowEllipse box = element as WireFlowEllipse;
            if (box == null)
                return null;
            var result = new FlowElement()
            {
                Type = "ellipse",
                Name = string.IsNullOrWhiteSpace(box.Name) ? null : box.Name,
                Left = Canvas.GetLeft(box),
                Top = Canvas.GetTop(box),
                Height = box.Height,
                Width = box.Width,
                Fill = box.Fill?.ToString(),
                HorizontalAlignment = box.HorizontalAlignment.ToFlow(),
                VerticalAlignment = box.VerticalAlignment.ToFlow(),
                Stroke = box.Stroke?.ToString(),
                StrokeThickness = box.StrokeThickness,
                Transparency = 1 - box.Opacity,
                Href = box.Href,
            };
            return result;
        }


        FlowElement ConvertUnknown(UIElement uiElement)
        {
            Debug.WriteLine($"UNKNOWN UI Element: {uiElement.GetType().Name}");
            return null;
        }


        #endregion



        #region Rendering


        public void RenderPage(Canvas canvas, FlowPage pageNode)
        {
            Unfollow(canvas);
            canvas.Children.Clear();
            if (pageNode == null)
            {
                RenderNodeProxy(canvas);
                return;
            }
            canvas.Width = !double.IsNaN(pageNode.Width) ? pageNode.Width : Project.Width;
            canvas.Height = !double.IsNaN(pageNode.Height) ? pageNode.Height : Project.Height;
            canvas.Background = BackgroundBrush(!string.IsNullOrWhiteSpace(pageNode.Background) ? pageNode.Background : InheritedPageBackground(pageNode));

            layerNo = 0;

            RenderMasterPages(pageNode.Master, canvas);
            RenderUserPage(pageNode, canvas);
            RenderGrids(canvas);

            // The page's size and background follow its node
            PropertyChangedEventHandler onPage = (sender, e) => ApplyPage(pageNode, canvas);
            pageNode.PropertyChanged += onPage;
            canvas.SetValue(ViewLinkProperty, new ViewLink { Node = pageNode, OnProperty = onPage });
        }


        // A page's size and background, its own or inherited, given to the canvas and to the layouts and layers on it
        private void ApplyPage(FlowPage pageNode, Canvas canvas)
        {
            var size = InheritedPageSize(pageNode);
            canvas.Width = !double.IsNaN(pageNode.Width) ? pageNode.Width : size.Width;
            canvas.Height = !double.IsNaN(pageNode.Height) ? pageNode.Height : size.Height;
            canvas.Background = BackgroundBrush(!string.IsNullOrWhiteSpace(pageNode.Background) ? pageNode.Background : InheritedPageBackground(pageNode));
            foreach (var layout in canvas.Children.OfType<Layout>())
            {
                layout.Width = canvas.Width;
                layout.Height = canvas.Height;
                foreach (var layer in layout.Children.OfType<Layer>())
                {
                    layer.Width = canvas.Width;
                    layer.Height = canvas.Height;
                }
            }
        }


        private void RenderNodeProxy(Canvas canvas)
        {
        }




        private void RenderUserPage(FlowPage node, Canvas canvas)
        {
            Debug.Assert(node.Name != null, "Null page to render.");
            Debug.Assert(!string.IsNullOrWhiteSpace(node.Name), "Repository returned page without a name.");
            node.IsMaster = false;
            Layout layout = new Layout()
            {
                Name = "Page",
                Width = canvas.Width,
                Height = canvas.Height,
                NodeName = node.Name,
            };
            canvas.Children.Add(layout);
            RenderNode<Layer>(node, layout);
        }



        private void RenderMasterPages(string nodeName, Canvas canvas)
        {
            var node = Repository.GetMasterPage(nodeName);
            if (node == null)
                return;
            Debug.Assert(!string.IsNullOrWhiteSpace(node.Name), "Repository returned master page without a name.");
            node.IsMaster = true;
            if (node.Master != null)
                RenderMasterPages(node.Master, canvas);

            Layout layout = new Layout()
            {
                Name = "Master",
                Width = canvas.Width,
                Height = canvas.Height,
                NodeName = nodeName,
            };
            canvas.Children.Add(layout);
            RenderNode<Master>(node, layout);
        }





        private Canvas RenderNode<LayerType>(FlowPage pageNode, Canvas canvas)
            where LayerType : Layer, new()
        {
            if (pageNode == null)
                return null;
            if (!Double.IsNaN(pageNode.Width))
                canvas.Width = pageNode.Width;
            if (!Double.IsNaN(pageNode.Height))
                canvas.Height = pageNode.Height;
            Canvas pageCanvas = canvas;
            RenderLayers<LayerType>(pageNode, pageCanvas);
            return pageCanvas;
        }



        private void RenderLayers<LayerType>(FlowPage pageNode, Canvas canvas)
            where LayerType : Layer, new()
        {
            if (pageNode == null || pageNode.Layers == null)
                return;
            foreach (var layer in pageNode.Layers)
                RenderLayer<LayerType>(pageNode, layer, canvas);
        }





        private void RenderLayer<LayerType>(FlowPage pageNode, FlowLayer layer, Canvas canvas)
            where LayerType: Layer, new()
        {
            Layer layerCanvas = new LayerType()
            {
                Width = canvas.Width,
                Height = canvas.Height,
                Name = layer.Name,
                PageNode = pageNode,
            };
            layerNo++;
            if (string.IsNullOrWhiteSpace(layer.Name))
                layerCanvas.Name = $"Layer{layerNo}";

            Canvas.SetTop(layerCanvas, 0);
            Canvas.SetLeft(layerCanvas, 0);

            canvas.Children.Add(layerCanvas);
            FlowView.SetNode(layerCanvas, layer);

            foreach (var element in layer.Elements)
            {
                RenderElement(element, layerCanvas);
            }
            FollowChildren(layer, layer.Elements, layerCanvas);
        }



        #region Following the DOM


        /// <summary>
        /// What the engine listens to for a page, layer or element it drew, so that it can stop when the drawing is
        /// taken away.
        /// </summary>
        private sealed class ViewLink
        {
            public FlowBase Node;
            public PropertyChangedEventHandler OnProperty;
            public INotifyCollectionChanged Children;
            public NotifyCollectionChangedEventHandler OnChildren;
        }


        private static readonly DependencyProperty ViewLinkProperty =
            DependencyProperty.RegisterAttached("ViewLink", typeof(ViewLink), typeof(WireFlowEngine), new PropertyMetadata(null));


        // Keeps a panel's children in step with a node's children
        private void FollowChildren(FlowBase node, IList<FlowElementBase> children, Panel panel)
        {
            var link = panel.GetValue(ViewLinkProperty) as ViewLink ?? new ViewLink { Node = node };
            link.Children = (INotifyCollectionChanged)children;
            link.OnChildren = (sender, e) => Reconcile(children, panel);
            link.Children.CollectionChanged += link.OnChildren;
            panel.SetValue(ViewLinkProperty, link);
        }


        // Keeps an element in step with its node's properties
        private void FollowNode(FlowElementBase node, FrameworkElement ui)
        {
            var link = ui.GetValue(ViewLinkProperty) as ViewLink ?? new ViewLink { Node = node };
            link.OnProperty = (sender, e) => Refresh(node, ui);
            node.PropertyChanged += link.OnProperty;
            ui.SetValue(ViewLinkProperty, link);
        }


        /// <summary>Stops following the nodes of a drawing and of everything in it.</summary>
        private static void Unfollow(DependencyObject ui)
        {
            if (ui == null)
                return;
            if (ui.GetValue(ViewLinkProperty) is ViewLink link)
            {
                if (link.OnProperty != null)
                    link.Node.PropertyChanged -= link.OnProperty;
                if (link.Children != null)
                    link.Children.CollectionChanged -= link.OnChildren;
                ui.ClearValue(ViewLinkProperty);
            }
            if (ui is Panel panel)
                foreach (UIElement child in panel.Children)
                    Unfollow(child);
        }


        /// <summary>
        /// Puts a panel's children in step with a node's children: the elements of nodes added are drawn, those of nodes
        /// taken away removed, and all put in the nodes' order (bottom first).
        /// </summary>
        private void Reconcile(IList<FlowElementBase> nodes, Panel panel)
        {
            var shown = new Dictionary<FlowBase, UIElement>();
            foreach (UIElement child in panel.Children)
            {
                var node = FlowView.GetNode(child);
                if (node != null && !shown.ContainsKey(node))
                    shown[node] = child;
            }
            var wanted = new List<UIElement>();
            foreach (var node in nodes.ToList())
            {
                if (!shown.TryGetValue(node, out var ui))
                    ui = RenderElement(node, panel);
                if (ui != null)
                    wanted.Add(ui);
            }
            for (int i = panel.Children.Count - 1; i >= 0; i--)
            {
                var child = panel.Children[i];
                if (FlowView.GetNode(child) != null && !wanted.Contains(child))
                {
                    Unfollow(child);
                    panel.Children.RemoveAt(i);
                }
            }
            for (int i = 0; i < wanted.Count; i++)
                if (panel.Children.IndexOf(wanted[i]) != i)
                {
                    panel.Children.Remove(wanted[i]);
                    panel.Children.Insert(i, wanted[i]);
                }
            if (panel is Group group)
                ApplyGroupSize(group);
        }


        /// <summary>
        /// Brings an element up to date with its node, in place; an image or a part reference that names another image or
        /// part is drawn again.
        /// </summary>
        private void Refresh(FlowElementBase node, FrameworkElement ui)
        {
            var panel = ui.Parent as Panel;
            if (panel == null)
                return;
            bool redraw = (ui is ImageBox image && image.ImageFileName != (node as FlowImage)?.Image)
                || (ui is Part part && part.TemplateName != node.Name);
            if (redraw)
            {
                int index = panel.Children.IndexOf(ui);
                Unfollow(ui);
                panel.Children.Remove(ui);
                var redrawn = RenderElement(node, panel);
                if (redrawn != null)
                {
                    panel.Children.Remove(redrawn);
                    panel.Children.Insert(Math.Min(index, panel.Children.Count), redrawn);
                }
            }
            else
                Apply(node, ui, panel);
            if (panel is Group group)
                ApplyGroupSize(group);
        }


        #endregion



        /// <summary>
        /// Draws an element in a panel, on top of what it holds, and keeps the drawing in step with the node; returns it,
        /// or null for an element that is not drawn.
        /// </summary>
        private FrameworkElement RenderElement(FlowElementBase element, Panel canvas)
        {
            try
            {
                FrameworkElement ui = CreateElement(element);
                if (ui == null)
                    return null;
                FlowView.SetNode(ui, element);
                Apply(element, ui, canvas);
                canvas.Children.Add(ui);
                AddNavigation(element, ui);
                FollowNode(element, ui);
                return ui;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.GetType()}\n{ex.Message}\nSTACK TRACE:\n{ex.StackTrace}");
                return null;
            }
        }


        // The control that draws a kind of element, with the drawings of its children (a group's, a stack panel's, a part's)
        private FrameworkElement CreateElement(FlowElementBase element)
        {
            switch (element.Type)
            {
                case "image":
                    return CreateImage(element as FlowImage);
                case "rect":
                    return new WireFlowRectangle();
                case "ellipse":
                    return new WireFlowEllipse();
                case "text":
                    return new WireFlowText();
                case "box":
                    return new Box();
                case "group":
                    return CreateGroup(element as FlowGroup);
                case "stack":
                    return CreateStack(element as FlowStack);
                case "part":
                    return CreatePartRef(element);
                case "none":
                    return null;
                default:
                    return new Rectangle { Name = "UnknownObject_" + element.Type };
            }
        }


        // Gives an element's control the values of its node
        private void Apply(FlowElementBase element, FrameworkElement ui, Panel panel)
        {
            switch (ui)
            {
                case WireFlowText text:
                    ApplyText(element as FlowElement, text);
                    break;
                case WireFlowRectangle rect:
                    ApplyRectangle(element as FlowElement, rect);
                    break;
                case WireFlowEllipse ellipse:
                    ApplyEllipse(element as FlowElement, ellipse);
                    break;
                case Box box:
                    ApplyBox(element as FlowElement, box);
                    break;
                case ImageBox image:
                    ApplyImage(element as FlowImage, image, panel);
                    break;
                case Stack stack:
                    ApplyStack(element as FlowStack, stack);
                    break;
                case Group group:
                    ApplyGroup(element, group);
                    break;
                case Part part:
                    ApplyPartRef(element, part);
                    break;
                case Rectangle unknown:
                    ApplyUnknown(element, unknown);
                    break;
            }
            ui.Cursor = string.IsNullOrWhiteSpace(element.Href) ? null : Cursors.Hand;
        }


        private void ApplyText(FlowElement element, WireFlowText text)
        {
            text.Opacity = 1 - element.Transparency;
            text.FontFamily = new FontFamily(element.Font);
            text.FontSize = element.FontSize;
            text.FontWeight = FontWeight.FromOpenTypeWeight(element.FontWeight);
            text.FontStretch = element.FontStretch.ToWpf();
            text.Foreground = ColorUtils.StringToBrush(element.Color);
            // A text's VerticalAlignment is where its lines lie down its frame (Stretch, as earlier texts have, is Top)
            text.TextVerticalAlignment = element.VerticalAlignment.ToWpf();
            // Its saved Fill; transparent without one (WireFlowText keeps it a brush, so that its whole frame can be clicked)
            text.Background = TextBackground(element.Fill);
            text.HorizontalAlignment = element.HorizontalAlignment.ToWpf();
            text.TextAlignment = element.TextAlignment.ToWpf();
            text.TextWrapping = element.TextWrapping.ToWpf();
            text.Text = element.Text;
            text.LineHeight = element.LineHeight;
            text.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            text.TextPadding = new Thickness(element.StrokeThickness);
            text.Href = element.Href;
            text.Width = element.Width;
            text.Height = element.Height;
            Canvas.SetLeft(text, element.Left);
            Canvas.SetTop(text, element.Top);
        }


        private void ApplyRectangle(FlowElement element, WireFlowRectangle rect)
        {
            // Kept, so that they stay in the element's node (a canvas ignores them)
            rect.HorizontalAlignment = element.HorizontalAlignment.ToWpf();
            rect.VerticalAlignment = element.VerticalAlignment.ToWpf();
            rect.Opacity = 1 - element.Transparency;
            rect.RadiusX = element.RadiusX;
            rect.RadiusY = element.RadiusY;
            rect.StrokeThickness = element.StrokeThickness;
            rect.Fill = new SolidColorBrush(ColorUtils.StringToColor(element.Fill, "LightGray"));
            rect.Stroke = new SolidColorBrush(ColorUtils.StringToColor(element.Stroke));
            rect.Href = element.Href;
            rect.Width = element.Width;
            rect.Height = element.Height;
            Canvas.SetLeft(rect, element.Left);
            Canvas.SetTop(rect, element.Top);
        }


        private void ApplyEllipse(FlowElement element, WireFlowEllipse ellipse)
        {
            // Kept, so that they stay in the element's node (a canvas ignores them)
            ellipse.HorizontalAlignment = element.HorizontalAlignment.ToWpf();
            ellipse.VerticalAlignment = element.VerticalAlignment.ToWpf();
            ellipse.Opacity = 1 - element.Transparency;
            ellipse.StrokeThickness = element.StrokeThickness;
            ellipse.Fill = new SolidColorBrush(ColorUtils.StringToColor(element.Fill, "LightGray"));
            ellipse.Stroke = new SolidColorBrush(ColorUtils.StringToColor(element.Stroke));
            ellipse.Href = element.Href;
            ellipse.Width = element.Width;
            ellipse.Height = element.Height;
            Canvas.SetLeft(ellipse, element.Left);
            Canvas.SetTop(ellipse, element.Top);
        }


        /// <summary>
        /// Rectangle with a stroke and fill, and a text element inside, may have rounded corners and a background image
        /// It may also serve as a link.
        /// </summary>
        private void ApplyBox(FlowElement element, Box box)
        {
            box.Width = element.Width;
            box.Height = element.Height;
            box.Left = element.Left;
            box.Top = element.Top;
            box.Text = element.Text;
            box.Opacity = 1 - element.Transparency;
            box.BorderColor = new SolidColorBrush(ColorUtils.StringToColor(element.Stroke, "Black"));
            box.BorderRadiusX = element.RadiusX;
            box.BorderRadiusY = element.RadiusY;
            box.BorderWidth = element.StrokeThickness;
            box.Fill = new SolidColorBrush(ColorUtils.StringToColor(element.Fill, "LightGray"));
            box.Foreground = ColorUtils.StringToBrush(element.Color);
            box.FontFamily = new FontFamily(element.Font);
            box.FontSize = element.FontSize;
            box.FontStretch = element.FontStretch.ToWpf();
            box.FontWeight = FontWeight.FromOpenTypeWeight(element.FontWeight);
            box.TextVerticalAlignment = element.VerticalAlignment.ToWpf();
            box.TextHorizontalAlignment = element.HorizontalAlignment.ToWpf();
            box.TextAlignment = element.TextAlignment.ToWpf();
            box.TextWrapping = element.TextWrapping.ToWpf();
            box.TextPadding = new Thickness(element.StrokeThickness);
            box.LineHeight = element.LineHeight;
            box.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            box.Href = element.Href;
        }


        //private void RenderTextBox(FlowElement element, Panel canvas)
        //{
        //    var box = new Box()
        //    {
        //        Width = element.Width,
        //        Height = element.Height,
        //    };
        //    Canvas.SetTop(box, element.Top);
        //    Canvas.SetLeft(box, element.Left);
        //    canvas.Children.Add(box);
        //}





        // A part reference, with the drawing of the part's saved content
        private Part CreatePartRef(FlowElementBase element)
        {
            var partCanvas = new Part { TemplateName = element.Name };
            var part = Repository.GetPart(element.Name);
            if (part != null)
                RenderElement(part, partCanvas);
            return partCanvas;
        }


        private void ApplyPartRef(FlowElementBase element, Part partCanvas)
        {
            partCanvas.Left = element.Left;
            partCanvas.Top = element.Top;
            partCanvas.Href = element.Href;
            // Without a size of its own, it takes the part's
            var part = partCanvas.Children.OfType<FrameworkElement>().Select(FlowView.GetNode).OfType<FlowElementBase>().FirstOrDefault();
            partCanvas.Width = double.IsNaN(element.Width) && part != null ? part.Width : element.Width;
            partCanvas.Height = double.IsNaN(element.Height) && part != null ? part.Height : element.Height;
        }



        private Group CreateGroup(FlowGroup element)
        {
            if (element.Children == null)
                throw new MissingMemberException("Group with no children!");
            var group = new Group();
            foreach (var child in element.Children)
                RenderElement(child, group);
            FollowChildren(element, element.Children, group);
            return group;
        }


        private void ApplyGroup(FlowElementBase element, Group group)
        {
            group.Left = element.Left;
            group.Top = element.Top;
            group.Href = element.Href;
            ApplyGroupSize(group);
        }


        // A group is as large as the extent of its children, from its own top left corner
        private static void ApplyGroupSize(Group group)
        {
            double width = 0.0, height = 0.0;
            foreach (var child in group.Children.OfType<FrameworkElement>())
            {
                if (!(FlowView.GetNode(child) is FlowElementBase node))
                    continue;
                width = Math.Max(width, node.Left + (double.IsNaN(node.Width) ? 0 : node.Width));
                height = Math.Max(height, node.Top + (double.IsNaN(node.Height) ? 0 : node.Height));
            }
            group.Width = width;
            group.Height = height;
        }


        /// <summary>
        /// A stack panel: its children one after another in its direction, stretched across it (see Stack)
        /// </summary>
        private Stack CreateStack(FlowStack element)
        {
            if (element == null)
                return null;
            var stack = new Stack();
            foreach (var child in element.Children)
                RenderElement(child, stack);
            FollowChildren(element, element.Children, stack);
            return stack;
        }


        private void ApplyStack(FlowStack element, Stack stack)
        {
            stack.Left = element.Left;
            stack.Top = element.Top;
            stack.Width = element.Width;
            stack.Height = element.Height;
            stack.Direction = element.Direction.ToWpf();
            stack.Background = new SolidColorBrush(ColorUtils.StringToColor(element.Fill, "Transparent"));
            stack.Opacity = 1 - element.Transparency;
            stack.Href = element.Href;
        }




        /// <summary>
        /// Rectangle with a stroke and fill, and a text element inside
        /// </summary>
        private void ApplyUnknown(FlowElementBase element, Rectangle rect)
        {
            rect.Width = element.Width;
            rect.Height = element.Height;
            Canvas.SetTop(rect, element.Top);
            Canvas.SetLeft(rect, element.Left);

            rect.Fill = ColorUtils.StringToBrush("Pink");
            rect.Stroke = ColorUtils.StringToBrush("Red");
            rect.StrokeThickness = 2;
            rect.RadiusX = rect.RadiusY;
            rect.Opacity = 1 - element.Transparency;
        }


        //private void RenderLinks(FlowPageNode pageNode, Canvas canvas)
        //{
        //    if (pageNode.Links == null || !pageNode.Links.Any())
        //        return;


        //    Canvas layerCanvas = new Canvas()
        //    {
        //        Width = canvas.Width,
        //        Height = canvas.Height,
        //    };
        //    Canvas.SetTop(layerCanvas, 0);
        //    Canvas.SetLeft(layerCanvas, 0);
        //    canvas.Children.Add(layerCanvas);

        //    SolidColorBrush brush = new SolidColorBrush();
        //    //brush.Color = Color.FromArgb(68, 220, 20, 60);
        //    brush.Color = Color.FromArgb(68, 20, 220, 60);

        //    layerCanvas.Resources.Add("background-brush", brush);

        //    foreach (var link in pageNode.Links)
        //    {
        //        Rectangle rect = new Rectangle();


        //        if (String.IsNullOrWhiteSpace(link.Href))
        //        {
        //            rect.StrokeThickness = 2;
        //            rect.Stroke = Brushes.OrangeRed;
        //            rect.Cursor = Cursors.Help;
        //        }
        //        else
        //        {
        //            rect.Cursor = Cursors.Hand;
        //        }


        //        rect.Fill = brush;
        //        layerCanvas.Children.Add(rect);
        //        rect.Width = link.Width;
        //        rect.Height = link.Height;

        //        Canvas.SetTop(rect, link.Top);
        //        Canvas.SetLeft(rect, link.Left);

        //        AddNavigation(link, rect);
        //    }
        //}

        private Canvas RenderGrids(Canvas canvas)
        {
            Guides gridLayer = new Guides()
            {
                Width = canvas.Width,
                Height = canvas.Height,
                Name = "Grids"
            };
            // AddMiddleCrossHair(gridLayer);

            if (ShowGrid)
                AddGrid(gridLayer);

            if (ShowGoldenRatios)
                AddGoldenRatios(gridLayer);
            if (ShowThirds)
                AddRuleOfThirds(gridLayer);
            if (Show4x4Grid)
                Add4x4Grid(gridLayer);
            if (Show5x5Grid)
                Add5x5Grid(gridLayer);
            if (Show7x7Grid)
                Add7x7Grid(gridLayer);
            if (Show12x12Grid)
                Add12x12Grid(gridLayer);

            if (gridLayer.Children.Count > 0)
            {
                canvas.Children.Add(gridLayer);
                return gridLayer;
            }
            return null;
        }


        #endregion




        #region Graphics Primitives


        // A text's background: its saved Fill, or transparent when it has none or one that is not a color
        // (ColorUtils.StringToBrush makes an empty Fill black)
        private static Brush TextBackground(string fill)
        {
            if (string.IsNullOrWhiteSpace(fill))
                return Brushes.Transparent;
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill));
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
                return Brushes.Transparent;
            }
        }


        // A click on a linked element follows its link, as its node has it when clicked; the innermost link wins
        private void AddNavigation(FlowElementBase link, FrameworkElement parent)
        {
            // Hosts that follow links (the Browser) set NavigateToNodeFunc; the Designer
            // leaves it null and handles clicks through its own navigate tool.
            parent.MouseLeftButtonUp += (sender, e) =>
            {
                if (NavigateToNodeFunc == null || string.IsNullOrWhiteSpace(link.Href))
                    return;
                e.Handled = true;
                NavigateToNodeFunc(link.Href);
            };
        }


        // An image's control, with its picture; without one (an image not imported), an empty frame of its size
        private ImageBox CreateImage(FlowImage element)
        {
            if (element == null)
                return null;
            var imageBits = String.IsNullOrWhiteSpace(element.Image) ? null : RetrieveImageBitmap(element);
            return new ImageBox(imageBits) { ImageFileName = element.Image };
        }


        private void ApplyImage(FlowImage element, ImageBox imageBox, Panel panel)
        {
            imageBox.Left = element.Left;
            imageBox.Top = element.Top;
            imageBox.Width = element.Width;
            imageBox.Height = element.Height;
            imageBox.Href = element.Href;
            if (element.Stretch != FlowStretch.None)
            {
                imageBox.Stretch = element.Stretch.ToWpf();
                if (Double.IsNaN(element.Width))
                    imageBox.Width = panel.Width;
                if (Double.IsNaN(element.Height))
                    imageBox.Height = panel.Height;
            }
            else
            {
                imageBox.Stretch = Stretch.None;
                // Without a size of its own, it takes its picture's
                if (Double.IsNaN(element.Width) && imageBox.Source != null)
                    imageBox.Source = imageBox.Source;
            }
        }




        private BitmapImage RetrieveImageBitmap(FlowImage element)
        {
            BitmapImage bi = new BitmapImage();
            MemoryStream ms = new MemoryStream(); //TODO: Creating a memory leak

            var bytArray = Repository.LoadImage(element.Image);
            if (bytArray == null)
                return null;

            ms.Write(bytArray, 0, bytArray.Length);
            ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        #endregion







        #region Grids

        public void AddGrid(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddGridLines(grid);
            AddGridLabels(grid);
        }


        public void AddGoldenRatios(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddGoldenRatioLines(grid);
        }


        private void AddGoldenRatioLines(Panel parent)
        {
            const double GOLDEN_RATIO = 1.61803398875;


            Brush brush = Brushes.Goldenrod;
            double thickness = 1;
            double excess = 6;
            double opacity = 1.0;

            double X = parent.Width / GOLDEN_RATIO;
            Line vLine;
            vLine = new Line()
            {
                X1 = X,
                X2 = X,
                Y1 = -excess,
                Y2 = parent.Height + excess,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = opacity
            };
            parent.Children.Add(vLine);
            vLine = new Line()
            {
                X1 = parent.Width - X,
                X2 = parent.Width - X,
                Y1 = -excess,
                Y2 = parent.Height + excess,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = opacity
            };
            parent.Children.Add(vLine);

            double Y = parent.Height / GOLDEN_RATIO;
            Line hLine;
            hLine = new Line()
            {
                X1 = -excess,
                X2 = parent.Width + excess,
                Y1 = Y,
                Y2 = Y,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = opacity
            };
            parent.Children.Add(hLine);
            hLine = new Line()
            {
                X1 = -excess,
                X2 = parent.Width + excess,
                Y1 = parent.Height - Y,
                Y2 = parent.Height - Y,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = opacity
            };
            parent.Children.Add(hLine);
        }





        private void AddGridLines(Panel parent, double gridSize = 20)
        {
            Brush brush;
            double thickness;
            double excess;

            Line vLine;
            for (double X = 0; X < parent.Width + 1; X += gridSize)
            {
                if (Math.Abs(X % 100) < Constants.EPSILON)
                {
                    brush = Brushes.Gray;
                    thickness = 1.5;
                    excess = 6;
                }
                else
                {
                    brush = Brushes.LightGray;
                    thickness = 0.5;
                    excess = 6;
                }
                vLine = new Line()
                {
                    X1 = X,
                    X2 = X,
                    Y1 = -excess,
                    Y2 = parent.Height + excess,
                    Stroke = brush,
                    StrokeThickness = thickness,
                    Opacity = 0.5,
                };

                parent.Children.Add(vLine);
            }

            Line hLine;
            for (double Y = 0; Y < parent.Height + 1; Y += gridSize)
            {
                if (Math.Abs(Y % 100) < Constants.EPSILON)
                {
                    brush = Brushes.Gray;
                    thickness = 1.5;
                    excess = 6;
                }
                else
                {
                    brush = Brushes.LightGray;
                    thickness = 0.5;
                    excess = 6;
                }

                hLine = new Line()
                {
                    X1 = -excess,
                    X2 = parent.Width + excess,
                    Y1 = Y,
                    Y2 = Y,
                    Stroke = brush,
                    StrokeThickness = thickness,
                    Opacity = 0.5,
                };
                parent.Children.Add(hLine);
            }
        }


        private void AddGridLabels(Panel parent)
        {
            const double GRID_SIZE = 20;
            TextBlock text;

            // Horizontal Axes
            for (double X = 0; X < parent.Width + 1; X += GRID_SIZE)
            {
                if (Math.Abs(X % 100) > Constants.EPSILON)
                    continue;

                text = new TextBlock()
                {
                    Text = X.ToString(CultureInfo.CurrentCulture),
                    Foreground = Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, X - (text.ActualWidth / 2));
                Canvas.SetTop(text, -text.ActualHeight - 6);
                Canvas.SetTop(text, -text.ActualHeight - 6);
                parent.Children.Add(text);


                text = new TextBlock()
                {
                    Text = X.ToString(CultureInfo.CurrentCulture),
                    Foreground = Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, X - (text.ActualWidth / 2));
                Canvas.SetTop(text, parent.Height + 3);
                parent.Children.Add(text);
            }


            // Vertical Axes
            for (double Y = 0; Y < parent.Height + 1; Y += GRID_SIZE)
            {
                if (Math.Abs(Y % 100) > Constants.EPSILON)
                    continue;
                //
                text = new TextBlock()
                {
                    Text = Y.ToString(CultureInfo.CurrentCulture),
                    Foreground = Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, -text.ActualWidth - 8);
                Canvas.SetTop(text, Y - (text.ActualHeight / 2) - 1);
                parent.Children.Add(text);
                //
                text = new TextBlock()
                {
                    Text = Y.ToString(CultureInfo.CurrentCulture),
                    Foreground = Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, parent.Width + 8);
                Canvas.SetTop(text, Y - (text.ActualHeight / 2) - 1);
                parent.Children.Add(text);

            }


        }



        public void AddRuleOfThirds(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddXbyYLines(parent, 3, 3, Brushes.Magenta);
            AddXbyYLabels(grid, 3, 3, Brushes.Magenta);
        }


        public void Add4x4Grid(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddXbyYLines(grid, 4, 4, Brushes.Teal);
            AddXbyYLabels(grid, 4, 4, Brushes.Teal);
        }



        public void Add5x5Grid(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddXbyYLines(grid, 5, 5, Brushes.LawnGreen);
            AddXbyYLabels(grid, 5, 5, Brushes.LawnGreen);
        }



        public void Add7x7Grid(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddXbyYLines(grid, 7, 7, Brushes.Cyan);
            AddXbyYLabels(grid, 7, 7, Brushes.Cyan);
        }


        public void Add12x12Grid(Panel parent)
        {
            Canvas grid = new Canvas()
            {
                Width = parent.Width,
                Height = parent.Height
            };
            parent.Children.Add(grid);
            AddXbyYLines(grid, 12, 12, Brushes.CadetBlue);
            AddXbyYLabels(grid, 12, 12, Brushes.CadetBlue);
        }



        private void AddXbyYLines(Panel parent, int x, int y, Brush brush)
        {
            double thickness = 0.75;
            double excess = 0;
            double opacity = 1.0;


            Line vLine;
            double increment;

            // Horizontal Lines
            increment = parent.Width / x;
            for (double X = 1; X < x; X++)
            {
                vLine = new Line()
                {
                    X1 = X * increment,
                    X2 = X * increment,
                    Y1 = -excess,
                    Y2 = parent.Height + excess,
                    Stroke = brush,
                    StrokeThickness = thickness,
                    Opacity = opacity
                };
                parent.Children.Add(vLine);
            }

            // Vertical Lines
            increment = parent.Height / y;
            for (double Y = 1; Y < y; Y++)
            {
                vLine = new Line()
                {
                    X1 = -excess,
                    X2 = parent.Width + excess,
                    Y1 = Y * increment,
                    Y2 = Y * increment,
                    Stroke = brush,
                    StrokeThickness = thickness,
                    Opacity = opacity
                };
                parent.Children.Add(vLine);
            }
        }



        private void AddXbyYLabels(Panel parent, int x, int y, Brush brush)
        {

            TextBlock text;
            double increment;

            // Horizontal Lines
            increment = parent.Width / x;
            for (double X = 1; X < x; X++)
            {
                // Top edge
                var value = X * increment;
                text = new TextBlock()
                {
                    Text = $"{X:N0}: {value:N0}",
                    Foreground = brush,
                    TextAlignment = TextAlignment.Center
                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, value - (text.ActualWidth / 2));
                Canvas.SetTop(text, -text.ActualHeight - 6);
                parent.Children.Add(text);

                // Bottom Edge
                text = new TextBlock()
                {
                    Text = $"{X:N0}: {value:N0}",
                    Foreground = brush,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, value - (text.ActualWidth / 2));
                Canvas.SetTop(text, parent.Height + 3);
                parent.Children.Add(text);
            }

            // Vertical Lines
            increment = parent.Height / y;
            for (double Y = 1; Y < y; Y++)
            {
                // Left Edge
                var value = Y * increment;
                text = new TextBlock()
                {
                    Text = $"{Y:N0}: {value:N0}",
                    Foreground = brush,
                    TextAlignment = TextAlignment.Center
                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, -text.ActualWidth - 8);
                Canvas.SetTop(text, value - (text.ActualHeight / 2) - 1);
                parent.Children.Add(text);
                // Right edge
                text = new TextBlock()
                {
                    Text = $"{Y:N0}: {value:N0}",
                    Foreground = brush,
                    TextAlignment = TextAlignment.Center

                };
                text.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                text.Arrange(new Rect(text.DesiredSize));
                Canvas.SetLeft(text, parent.Width + 8);
                Canvas.SetTop(text, value - (text.ActualHeight / 2) - 1);
                parent.Children.Add(text);
            }
        }


        public void AddMiddleCrossHair(Panel parent)
        {
            const double CIRCLE_RADIUS = 8;
            const double CROSSHAIR_SIZE = 12;


            Ellipse circle = new Ellipse()
            {
                Width = CIRCLE_RADIUS,
                Height = CIRCLE_RADIUS,
                //Fill = Brushes.Crimson,
                Stroke = Brushes.Crimson,
                StrokeThickness = 1
            };
            Canvas.SetLeft(circle, parent.Width / 2 - CIRCLE_RADIUS / 2);
            Canvas.SetTop(circle, parent.Height / 2 - CIRCLE_RADIUS / 2);
            parent.Children.Add(circle);


            Line vline = new Line()
            {
                X1 = parent.Width / 2,
                X2 = parent.Width / 2,
                Y1 = parent.Height / 2 - CROSSHAIR_SIZE,
                Y2 = parent.Height / 2 + CROSSHAIR_SIZE,
                Stroke = Brushes.Crimson,
                StrokeThickness = 1
            };
            parent.Children.Add(vline);


            Line hline = new Line()
            {
                X1 = parent.Width / 2 - CROSSHAIR_SIZE,
                X2 = parent.Width / 2 + CROSSHAIR_SIZE,
                Y1 = parent.Height / 2,
                Y2 = parent.Height / 2,
                Stroke = Brushes.Crimson,
                StrokeThickness = 1
            };
            parent.Children.Add(hline);
        }

        #endregion






        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
