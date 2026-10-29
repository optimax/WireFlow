using System;
using System.Collections;
using System.Collections.Generic;
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

            var layout = UserLayout(canvas);

            return SerializeLayout(page, layout);
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
                case "rectangle":
                    element = ConvertRectangle(uiElement);
                    break;
                case "ellipse":
                    element = ConvertEllipse(uiElement);
                    break;
                case "wireflowtextblock":
                case "textblock":
                    element = ConvertTextBlock(uiElement);
                    break;
                case "group":
                    element = ConvertGroup(uiElement);
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
            };
            foreach (var uiChild in group.Children.Cast<UIElement>())
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
                FontStretch = box.FontStretch,
                HorizontalAlignment = box.TextHorizontalAlignment,
                VerticalAlignment = box.TextVerticalAlignment,
                LineHeight = box.LineHeight,
                RadiusX = box.BorderRadiusX,
                RadiusY = box.BorderRadiusY,
                Stroke = box.BorderColor.ToString(),
                StrokeThickness = box.BorderWidth,
                Text = box.Text,
                TextAlignment = box.TextAlignment,
                TextWrapping = box.TextWrapping,
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
            TextBlock block = element as TextBlock;
            if (block == null)
                return null;
            var result = new FlowElement()
            {
                Type = "text",
                Name = string.IsNullOrWhiteSpace(block.Name) ? null : block.Name,
                Left = Canvas.GetLeft(block),
                Top = Canvas.GetTop(block),
                Height = block.Height,
                Width = block.Width,
                Color = block.Foreground?.ToString(),
                Fill = block.Background?.ToString(),
                Font = block.FontFamily?.ToString(),
                FontSize = block.FontSize,
                FontStretch = block.FontStretch,
                FontWeight = block.FontWeight.ToOpenTypeWeight(),
                HorizontalAlignment = block.HorizontalAlignment,
                VerticalAlignment = block.VerticalAlignment,
                LineHeight = block.LineHeight,
                Text = block.Text,
                TextAlignment = block.TextAlignment,
                TextWrapping = block.TextWrapping,
                Transparency = 1 - block.Opacity,
            };
            return result;
        }





        FlowElement ConvertRectangle(UIElement element)
        {
            Rectangle box = element as Rectangle;
            var result = new FlowElement()
            {
                Type = "rect",
                Name = string.IsNullOrWhiteSpace(box.Name) ? null : box.Name,
                Left = Canvas.GetLeft(box),
                Top = Canvas.GetTop(box),
                Height = box.Height,
                Width = box.Width,
                Fill = box.Fill?.ToString(),
                HorizontalAlignment = box.HorizontalAlignment,
                VerticalAlignment = box.VerticalAlignment,
                RadiusX = box.RadiusX,
                RadiusY = box.RadiusY,
                Stroke = box.Stroke?.ToString(),
                StrokeThickness = box.StrokeThickness,
                Transparency = 1 - box.Opacity,
            };
            return result;
        }



        FlowElement ConvertEllipse(UIElement element)
        {
            Ellipse box = element as Ellipse;
            var result = new FlowElement()
            {
                Type = "ellipse",
                Name = string.IsNullOrWhiteSpace(box.Name) ? null : box.Name,
                Left = Canvas.GetLeft(box),
                Top = Canvas.GetTop(box),
                Height = box.Height,
                Width = box.Width,
                Fill = box.Fill.ToString(),
                HorizontalAlignment = box.HorizontalAlignment,
                VerticalAlignment = box.VerticalAlignment,
                Stroke = box.Stroke.ToString(),
                StrokeThickness = box.StrokeThickness,
                Transparency = 1 - box.Opacity,
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
            canvas.Children.Clear();
            if (pageNode == null)
            {
                RenderNodeProxy(canvas);
                return;
            }
            canvas.Width = !double.IsNaN(pageNode.Width) ? pageNode.Width : Project.Width;
            canvas.Height = !double.IsNaN(pageNode.Height) ? pageNode.Height : Project.Height;

            layerNo = 0;

            RenderMasterPages(pageNode.Master, canvas);
            RenderUserPage(pageNode, canvas);
            RenderGrids(canvas);
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
            var typeName = typeof (LayerType).ToString();

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

            foreach (var element in layer.Elements)
            {
                RenderElement(element, layerCanvas);
            }
        }



        private void RenderElement(FlowElementBase element, Panel canvas)
        {
            try
            {
                string eltType = element.Type;
                switch (eltType)
                {
                    case "image":
                        RenderImage(element, canvas);
                        break;
                    case "rect":
                        RenderRectangle(element, canvas);
                        break;
                    case "text":
                        RenderText(element, canvas);
                        break;
                    case "box":
                        RenderBox(element, canvas);
                        break;
                    case "group":
                        RenderGroup(element, canvas);
                        break;
                    case "part":
                        RenderPartRef(element, canvas);
                        break;
                    case "none":
                        break;
                    default:
                        RenderUnknown(element, canvas);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.GetType()}\n{ex.Message}\nSTACK TRACE:\n{ex.StackTrace}");
            }
        }



        private void RenderImage(FlowElementBase element, Panel canvas)
        {
            var img = AddImage(element as FlowImage, canvas);
            AddNavigation(element, img);
        }


        private void RenderText(FlowElementBase element, Panel canvas)
        {
            var text = AddTextBlock(element as FlowElement, canvas);
            if (text == null)
                return;
            AddNavigation(element, text);
            text.Width = element.Width;
            text.Height = element.Height;
            Canvas.SetLeft(text, element.Left);
            Canvas.SetTop(text, element.Top);
        }


        private void RenderRectangle(FlowElementBase element, Panel canvas)
        {
            var rect = AddRectangle(element as FlowElement, canvas);
            AddNavigation(element, rect);
            rect.Width = element.Width;
            rect.Height = element.Height;
            Canvas.SetLeft(rect, element.Left);
            Canvas.SetTop(rect, element.Top);
        }


        /// <summary>
        /// Rectangle with a stroke and fill, and a text element inside, may have rounded corners and a background image
        /// It may also serve as a link.
        /// </summary>
        private void RenderBox(FlowElementBase baseElement, Panel canvas)
        {
            //var box = new Grid()
            //{
            //    Width = element.Width,
            //    Height = element.Height,
            //    ClipToBounds = true,
            //};
            //Canvas.SetTop(box, element.Top);
            //Canvas.SetLeft(box, element.Left);

            var element = baseElement as FlowElement;


            Color fillColor = ColorUtils.StringToColor(element.Fill, "LightGray");
            Color strokeColor = ColorUtils.StringToColor(element.Stroke, "Black");
            SolidColorBrush fill = new SolidColorBrush(fillColor);
            SolidColorBrush stroke = new SolidColorBrush(strokeColor);

            var box = new Box()
            {
                Width = element.Width,
                Height = element.Height,
                Left = element.Left,
                Top = element.Top,
                Text = element.Text,
                Opacity = 1 - element.Transparency,
                BorderColor = stroke,
                BorderRadiusX = element.RadiusX,
                BorderRadiusY = element.RadiusY,
                BorderWidth = element.StrokeThickness,
                Fill = fill,
                Foreground = ColorUtils.StringToBrush(element.Color),
                FontFamily = new FontFamily(element.Font),
                FontSize = element.FontSize,
                FontStretch = element.FontStretch,
                FontWeight = FontWeight.FromOpenTypeWeight(element.FontWeight),
                TextVerticalAlignment = element.VerticalAlignment,
                TextHorizontalAlignment = element.HorizontalAlignment,
                TextAlignment = element.TextAlignment,
                TextWrapping = element.TextWrapping,
                TextPadding = new Thickness(element.StrokeThickness),
                LineHeight = element.LineHeight,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                Href = element.Href,
            };
            canvas.Children.Add(box);
            AddNavigation(element, box);


            //AddImage(element, box);
            //AddRectangle(element, box);
            //AddTextBlock(element, box);
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





        private void RenderPartRef(FlowElementBase element, Panel canvas)
        {

            var partCanvas = new Part()
            {
                Width = element.Width,
                Height = element.Height,
                TemplateName = element.Name,
                Left = element.Left,
                Top = element.Top
            };
            canvas.Children.Add(partCanvas);

            var partName = element.Name;
            var part = Repository.GetPart(partName);
            if (part == null)
            {
                RenderNodeProxy(partCanvas);
                return;
            }
            RenderElement(part, partCanvas);
            if (double.IsNaN(element.Width))
                partCanvas.Width = part.Width;
            if (double.IsNaN(element.Height))
                partCanvas.Height = part.Height;

        }





        private void RenderGroup(FlowElementBase baseElement, Panel canvas)
        {
            var element = baseElement as FlowGroup;


            if (element.Children == null)
                throw new MissingMemberException("Group with no children!");
            var group = new Group()
            {
                Left = element.Left,
                Top = element.Top,
            };
            //
            double minX = Double.PositiveInfinity;
            double minY = Double.PositiveInfinity;
            double maxWidth = 0.0;
            double maxHeight = 0.0;
            foreach (var child in element.Children)
            {
                RenderElement(child, group);
                if (child.Left < minX)
                    minX = child.Left;
                if (child.Top < minY)
                    minY = child.Top;
                var extX = child.Left + child.Width;
                if (extX > maxWidth)
                    maxWidth = extX;
                var extY = child.Top + child.Height;
                if (extY > maxHeight)
                    maxHeight = extY;
            }
            group.Width = maxWidth;
            group.Height = maxHeight;
            //
            element.Left = minX;
            element.Top = minY;
            element.Width = maxWidth;
            element.Height = maxHeight;
            //
            canvas.Children.Add(group);
        }




        /// <summary>
        /// Rectangle with a stroke and fill, and a text element inside
        /// </summary>
        private void RenderUnknown(FlowElementBase element, Panel canvas)
        {
            var rect = new Rectangle()
            {
                Width = element.Width,
                Height = element.Height,
                Name = "UnknownObject_"+element.Type,
            };
            Canvas.SetTop(rect, element.Top);
            Canvas.SetLeft(rect, element.Left);

            Brush brush = ColorUtils.StringToBrush("Pink");
            Brush stroke = ColorUtils.StringToBrush("Red");

            rect.Fill = brush;
            rect.Stroke = stroke;
            rect.StrokeThickness = 2;
            rect.RadiusX = rect.RadiusY;
            rect.Opacity = 1 - element.Transparency;

            canvas.Children.Add(rect);
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


        private TextBlock AddTextBlock(FlowElement element, Panel panel)
        {
            if (String.IsNullOrWhiteSpace(element.Text))
                return null;

            var block = new WireFlowTextBlock
            {
                Width = double.NaN,
                Height = double.NaN,

                Opacity = 1 - element.Transparency,
                FontFamily = new FontFamily(element.Font),
                FontSize = element.FontSize,
                FontWeight = FontWeight.FromOpenTypeWeight(element.FontWeight),
                FontStretch = element.FontStretch,
                Foreground = ColorUtils.StringToBrush(element.Color),
                VerticalAlignment = element.VerticalAlignment,
                HorizontalAlignment = element.HorizontalAlignment,
                TextAlignment = element.TextAlignment,
                TextWrapping = element.TextWrapping,
                Text = element.Text,
                LineHeight = element.LineHeight,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                Padding = new Thickness(element.StrokeThickness),
            };
            panel.Children.Add(block);

            return block;
        }


        private Rectangle AddRectangle(FlowElement element, Panel panel)
        {
            Color fillColor = ColorUtils.StringToColor(element.Fill, "LightGray");
            SolidColorBrush fill = new SolidColorBrush(fillColor);
            Color strokeColor = ColorUtils.StringToColor(element.Stroke);
            SolidColorBrush stroke = new SolidColorBrush(strokeColor);

            var rect = new Rectangle()
            {
                Width = double.NaN,
                Height = double.NaN,

                Opacity = 1 - element.Transparency,
                RadiusX = element.RadiusX,
                RadiusY = element.RadiusY,
                StrokeThickness = element.StrokeThickness,
                Fill = fill,
                Stroke = stroke,
            };
            panel.Children.Add(rect);
            return rect;
        }


        private void AddNavigation(FlowElementBase link, FrameworkElement parent)
        {
            if (String.IsNullOrWhiteSpace(link.Href))
                return;


            parent.Cursor = Cursors.Hand;

            // Hosts that follow links (the Browser) set NavigateToNodeFunc; the Designer
            // leaves it null and handles clicks through its own navigate tool.
            parent.MouseLeftButtonUp += (sender, e) =>
            {
                if (NavigateToNodeFunc == null)
                    return;
                e.Handled = true;
                NavigateToNodeFunc(link.Href);
            };
        }


        private ImageBox AddImage(FlowImage element, Panel panel)
        {
            if (String.IsNullOrWhiteSpace(element?.Image))
                return null;

            var imageBits = RetrieveImageBitmap(element);
            if (imageBits == null)
                return null;



            ImageBox imageBox = new ImageBox(imageBits)
            {
                Left = element.Left,
                Top = element.Top,
                Width = element.Width,
                Height = element.Height,
                ImageFileName = element.Image,
            };


            if (element.Stretch != Stretch.None)
            {
                imageBox.Stretch = element.Stretch;
                if (Double.IsNaN(element.Width))
                    imageBox.Width = panel.Width;
                if (Double.IsNaN(element.Height))
                    imageBox.Height = panel.Height;
            }


            panel.Children.Add(imageBox);
            return imageBox;
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
