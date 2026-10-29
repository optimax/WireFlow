using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WireFlow.Annotations;

namespace WireFlow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private string documentFileName;
        private bool showNodeTitle = true;

        #endregion


        #region Properties

        public IFlowRepository Repository { get; } // Do not use directly. Use Engine.

        public WireFlowEngine Engine { get; }
        public FlowProject Project { get; set; }


        public string CurrentNodeName { get; set; }
        public FlowPage CurrentNode { get; set; }

        public int HistoryIndex { get; set; }

        public string DocumentFileName
        {
            get { return documentFileName; }
            set
            {
                if (documentFileName == value)
                    return;
                documentFileName = value;
                OnPropertyChanged(nameof(DocumentFileName));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }


        public ICommand ReloadFileCommand { get; }
        public ICommand RefreshViewCommand { get; }

        public ICommand NavigateBackCommand { get; }
        public ICommand NavigateForwardCommand { get; }

        public ICommand ShowTestWindowCommand { get; }



        public IList<string> History { get; }






        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(DocumentFileName))
                    return $"WireFlow Browser";
                return $"{DocumentFileName} - WireFlow Browser";
            }
        }

        public string NodeTitle
        {
            get
            {
                if (CurrentNodeName == null)
                    return "";
                if (CurrentNode == null)
                    return CurrentNodeName;
                //if (!string.IsNullOrWhiteSpace(CurrentNode.Title))
                //    return CurrentNode.Title;
                if (!string.IsNullOrWhiteSpace(CurrentNode.Name))
                    return CurrentNode.Name;
                return CurrentNodeName;
            }
        }

        public bool ShowNodeTitle
        {
            get { return showNodeTitle; }
            set
            {
                if (showNodeTitle == value)
                    return;
                showNodeTitle = value;
                OnPropertyChanged(nameof(ShowNodeTitle));
            }
        }

        public bool ShowCenter
        {
            get { return Engine.ShowCenter; }
            set { Engine.ShowCenter = value; }
        }

        public bool ShowGrid
        {
            get { return Engine.ShowGrid; }
            set { Engine.ShowGrid = value; }
        }

        public bool ShowThirds
        {
            get { return Engine.ShowThirds; }
            set { Engine.ShowThirds = value; }
        }


        public bool ShowGoldenRatios
        {
            get { return Engine.ShowGoldenRatios; }
            set { Engine.ShowGoldenRatios = value; }
        }


        public bool Show4x4Grid
        {
            get { return Engine.Show4x4Grid; }
            set { Engine.Show4x4Grid = value; }
        }

        public bool Show5x5Grid
        {
            get { return Engine.Show5x5Grid; }
            set { Engine.Show5x5Grid = value; }
        }


        public bool Show7x7Grid
        {
            get { return Engine.Show7x7Grid; }
            set { Engine.Show7x7Grid = value; }
        }


        public bool Show12x12Grid
        {
            get { return Engine.Show12x12Grid; }
            set { Engine.Show12x12Grid = value; }
        }

        #endregion



        #region Ctors

        public MainWindow()
        {
            History = new List<string>();
            ReloadFileCommand = new RelayCommand(ReloadFile);
            RefreshViewCommand = new RelayCommand(RefreshNode);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            NavigateForwardCommand = new RelayCommand(NavigateForward);
            ShowTestWindowCommand = new RelayCommand(ShowTestWindow);

            InitializeComponent();
            DataContext = this;



            Repository = new FlatFolderRepository();
            Repository.ProjectFileLoaded = s =>
            {
                DocumentFileName = s;
            };
            Engine = new WireFlowEngine(Repository);
            Engine.ProjectFileLoaded = s =>
            {
                DocumentFileName = s;
            };
            Engine.NavigateToNodeFunc = NavigateToNode;

            Engine.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged(args.PropertyName);
            };
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
            AddAdorners();
        }


        //Adorner gridAdorner;
        //Adorner crossAdorner;

        private void AddAdorners()
        {
            //AdornerLayer ad = AdornerLayer.GetAdornerLayer(ZoomBorder);
            //crossAdorner = new CrossAdorner(ZoomBorder);
            //ad.Add(crossAdorner);

            //// This layer does not scale
            //AdornerLayer ad = AdornerLayer.GetAdornerLayer(ZoomBorder.Child);
            //gridAdorner = new GridAdorner(ZoomBorder.Child);
            //ad.Add(gridAdorner);
        }

        #endregion





        #region Menu


        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open WireFlow Repository";
            dlg.Filter = Engine.DocumentFileFilters;
            if (dlg.ShowDialog() != true)
                return;
            OpenDocumentFile(dlg.FileName);
        }



        private void MenuItem_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Current Page As...";
            dlg.Filter = "XAML files|*.xaml";
            if (dlg.ShowDialog() != true)
                return;
            SaveAsXAML(dlg.FileName);
        }


        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
            Viewer.Reset();
        }



        private void MenuItem_ViewReset_Click(object sender, RoutedEventArgs e)
        {
            Viewer.Reset();
        }


        private void MenuItem_FitInWindow_Click(object sender, RoutedEventArgs e)
        {
            //ZoomBorder.FitInWindow();
        }






        private void MenuItem_SaveHistoryAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save History As...";
            dlg.Filter = "Text files|*.txt|All files|*";
            if (dlg.ShowDialog() != true)
                return;
            SaveHistoryToFile(dlg.FileName);
        }




        private void MenuItem_ViewToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            RefreshNode();
        }



        private void MenuItem_ClearHistory_Click(object sender, RoutedEventArgs e)
        {

            History.Clear();
            HistoryIndex = 0;

            OpenNode(CurrentNodeName);
        }


        private void MenuItem_TestBox_Click(object sender, RoutedEventArgs e)
        {
            //Canvas layer = new Canvas()
            //{
            //    Name= "Layer1",
            //    Width = Viewer.Canvas.Width,
            //    Height = Viewer.Canvas.Height,
            //};
            //Viewer.Canvas.Children.Add(layer);

            Canvas layer = Viewer.Canvas;


            var box = new Grid()
            {
                Width = 300,
                Height = 400,
                Background = Brushes.Red
            };
            //ColumnDefinition gridCol1 = new ColumnDefinition();
            //box.ColumnDefinitions.Add();
            Canvas.SetLeft(box, 150);
            Canvas.SetTop(box, 50);
            layer.Children.Add(box);


            Rectangle rect = new Rectangle()
            {
                Width = double.NaN,
                Height = double.NaN,
                Fill = Brushes.LightSkyBlue,
                Stroke = Brushes.White,
                StrokeThickness = 2,
            };
            box.Children.Add(rect);


            TextBlock text = new TextBlock()
            {
                Width = double.NaN,
                Height = double.NaN,
                Padding = new Thickness(3),
                Text = "This text should be displayed inside a blue rectangle and be centered both horizontally and vertically inside the rectangle.",
                FontFamily = new FontFamily("Impact"),
                FontSize = 30,
                FontWeight = FontWeight.FromOpenTypeWeight(400),
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            box.Children.Add(text);
        }


        private void MenuItem_TestWindow_Click(object sender, RoutedEventArgs e)
        {
            ShowTestWindow();
        }


        #endregion



        #region UI Events



        private void ToolBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = toolBar.HasOverflowItems ? Visibility.Visible : Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                var defaultMargin = new Thickness(0, 0, 11, 0);
                mainPanelBorder.Margin = toolBar.HasOverflowItems ? defaultMargin : new Thickness(0);
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Home:
                    //ZoomBorder.FitInWindow();
                    break;
                case Key.End:
                    Viewer.Reset();
                    break;
                case Key.G:
                    Engine.ShowGrid = !Engine.ShowGrid;
                    RefreshNode();
                    break;
                //case Key.F5:
                //    ReloadFile();
                //    break;
                //case Key.F6:
                //    RefreshNode();
                //    break;
            }
        }





        #endregion






        #region Methods

        private Canvas ResetCanvas(string colorStr = null)
        {
            Canvas canvas = Viewer.Canvas;
            canvas?.Children.Clear();
            if (canvas != null)
            {
                if (Project == null)
                {
                    canvas.Width = 0; //ZoomBorder.Width;
                    canvas.Height = 0; //ZoomBorder.Height;
                    canvas.Background = Brushes.Black;
                }
                else
                {
                    canvas.Width = Project.Width;
                    canvas.Height = Project.Height;
                    Color fillColor = ColorUtils.StringToColor(colorStr, Constants.DEFAULT_BACKGROUND);
                    Brush brush = new SolidColorBrush(fillColor);
                    canvas.Background = brush;
                }
            }
            Viewer.Reset();
            return canvas;
        }



        private void OpenDocumentFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            Project = Engine.OpenProject(fileName);
            if (Project == null)
                return;
            string homeNode = Project.Home;
            string background = Project.Background;
            ResetCanvas(background);
            OpenNode(homeNode);
        }



        private void OpenNode(string nodeName, bool addToHistory = true)
        {
            try
            {
                CurrentNode = null;
                CurrentNodeName = nodeName;
                OnPropertyChanged(nameof(NodeTitle));

                Canvas canvas = Viewer.Child as Canvas;
                canvas?.Children.Clear();


                if (string.IsNullOrWhiteSpace(nodeName))
                    return;
                if (addToHistory)
                    AddToHistory(nodeName);

                var node = Engine.GetPage(nodeName);
                Engine.RenderPage(canvas, node);

                CurrentNode = node;
                OnPropertyChanged(nameof(NodeTitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,ex.GetType().Name);

            }
        }


        private void ReloadFile()
        {
            if (string.IsNullOrWhiteSpace(DocumentFileName))
                return;

            OpenDocumentFile(DocumentFileName);
        }



        private void RefreshNode()
        {
            if (string.IsNullOrWhiteSpace(DocumentFileName))
                return;
            OpenNode(CurrentNodeName);
        }



        private void SaveAsXAML(string fileName)
        {
            Canvas canvas = Viewer.Child as Canvas;
            Transform rt = canvas.RenderTransform;
            try
            {
                canvas.RenderTransform = null;
                string mystrXAML = XamlWriter.Save(canvas);
                using (FileStream fs = File.Create(fileName))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(mystrXAML);
                        sw.Close();
                    }
                }
            }
            finally
            {
                canvas.RenderTransform = rt;
            }
        }



        private void ShowTestWindow()
        {
            TestWindow w = new TestWindow();
            w.Show();
        }



        #endregion




        #region Navigation


        private void NavigateBack()
        {
            if (HistoryIndex >= History.Count - 1)
                return;
            HistoryIndex++;
            var nodeName = History[HistoryIndex];
            OpenNode(nodeName);
            HistoryIndex++;
        }



        private void NavigateForward()
        {
            if (HistoryIndex <= 0)
                return;
            HistoryIndex--;
            var nodeName = History[HistoryIndex];
            OpenNode(nodeName, false);
        }



        private void NavigateToNode(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return;
            OpenNode(href);
            HistoryIndex = 0;
        }



        #endregion


        #region History


        private void AddToHistory(string nodeName)
        {
            if (History.FirstOrDefault() == nodeName)
                return;
            History.Insert(0, nodeName);
        }


        private void SaveHistoryToFile(string fileName)
        {
            using (TextWriter tw = new StreamWriter(fileName))
            {
                for (int i = 0; i < History.Count; i++)
                {
                    string s = History[i];
                    if (HistoryIndex == i)
                        tw.WriteLine(s + "*");
                    else
                        tw.WriteLine(s);
                }
            }
        }


        #endregion










        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
