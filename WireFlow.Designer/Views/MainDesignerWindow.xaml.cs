using Microsoft.Win32;
using Newtonsoft.Json;
using Optimax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Newtonsoft.Json.Serialization;
using WireFlow.Views;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Formatting = Newtonsoft.Json.Formatting;

namespace WireFlow
{
    public partial class MainDesignerWindow : Window, INotifyPropertyChanged
    {

        #region Properties

        public string ConfigFileName
        {
            get
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return System.IO.Path.Combine(folderPath, Globals.CONFIG_APP_NAME, Globals.CONFIG_FILE_NAME);
            }
        }

        public ConfigSettings Config { get; set; }

        public WireFlowEngine Engine { get; }


        private String FileFolder { get; set; }

        public String FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
                OnPropertyChanged("DesignerTitle");
            }
        }


        private bool IsProjectFileOpen => !string.IsNullOrWhiteSpace(FileName) && Engine.Project != null;
        private bool IsPageSelected => CurrentDesigner != null && CurrentDesigner.CurrentPage != null;






        public string CurrentPageName { get; set; }
        public string CurrentLayoutName { get; set; }
        public string CurrentPartName { get; set; }
        public string CurrentImageName { get; set; }


        //{
        //    get
        //    {
        //        //return CurrentDesigner?.PageName;
        //        return null;
        //    }
        //    set
        //    {
        //        //if (CurrentDesigner == null)
        //        //    return;
        //        //CurrentDesigner.PageName = value;
        //        //Debug.WriteLine($"Designer: {CurrentDesigner.AbsoluteId} PageName: {CurrentDesigner.PageName}-->{value}");
        //        //CurrentDesigner.NavigateToPage(value);
        //        //PopulateOutlineTree();
        //        //OnPropertyChanged(nameof(CurrentPageName));
        //    }
        //}



        public string DesignerTitle
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FileName))
                    return APP_NAME;
                return string.Format($"[{System.IO.Path.GetFileNameWithoutExtension(FileName)}] {APP_NAME} - {System.IO.Path.GetDirectoryName(FileName)}");
            }
        }


        public ObservableCollection<IDrawingTool> DrawingTools
        {
            get { return drawingTools; }
            set
            {
                if (drawingTools == value)
                    return;
                drawingTools = value;
                OnPropertyChanged(nameof(DrawingTools));
            }
        }


        public ObservableCollection<UIElementProxy> LayerOutline
        {
            get { return layerOutline; }
            set
            {
                if (layerOutline == value)
                    return;
                layerOutline = value;
                OnPropertyChanged(nameof(LayerOutline));
            }

        }

        public ObservableCollection<string> Pages
        {
            get { return pages; }
            set
            {
                if (pages == value)
                    return;
                pages = value;
                OnPropertyChanged(nameof(Pages));
            }
        }


        public ObservableCollection<string> Layouts
        {
            get { return layouts; }
            set
            {
                if (layouts == value)
                    return;
                layouts = value;
                OnPropertyChanged(nameof(Layouts));
            }
        }


        public ObservableCollection<string> Parts
        {
            get { return parts; }
            set
            {
                if (parts == value)
                    return;
                parts = value;
                OnPropertyChanged(nameof(Parts));
            }
        }



        public ObservableCollection<string> Images
        {
            get { return images; }
            set
            {
                if (images == value)
                    return;
                images = value;
                OnPropertyChanged(nameof(Images));
            }
        }


        public LayoutDocument CurrentDocument { get; set; }

        public int CurrentDesignerIndex {
            get
            {
                return currentDesignerIndex;
            }
            set
            {
                currentDesignerIndex = value;
                CurrentDocument = DocumentPane.SelectedContent as LayoutDocument;
                CurrentDesigner = CurrentDocument?.Content as DesignerNavigator;
            }
        }


        public DesignerNavigator CurrentDesigner
        {
            get { return currentDesigner; }
            set
            {
                if (currentDesigner == value)
                    return;
                currentDesigner = value;
                OnPropertyChanged(nameof(CurrentDesigner));
            }
        }

        public int SelectedToolIndex
        {
            get { return selectedToolIndex; }
            set
            {
                if (selectedToolIndex == value)
                    return;
                selectedToolIndex = value;
                if (CurrentDesigner != null)
                    CurrentDesigner.SelectedDrawingTool = Tools.Items[selectedToolIndex] as IDrawingTool;
                OnPropertyChanged(nameof(SelectedToolIndex));
            }
        }


        public IDrawingTool DefaultDrawingTool { get; set; }
        public IDrawingTool SelectedDrawingTool => CurrentDesigner?.SelectedDrawingTool;
        public Canvas CurrentCanvas => CurrentDesigner?.Designer?.Canvas;

        public FrameworkElement CurrentElement
        {
            get
            {
                return CurrentDesigner?.CurrentElement;
            }
            set
            {
                if (CurrentDesigner != null)
                    CurrentDesigner.CurrentElement = value;
            }
        }






        #endregion




        #region C'tors

        public MainDesignerWindow(WireFlowEngine engine)
        {
            InitializeComponent();
            DataContext = this;
            Engine = engine;
            //Engine.NavigateToNodeFunc = NavigateInNavMode;
            Engine.ProjectFileLoaded = s =>
            {
                FileName = s;
            };
            SetupDrawingTools();
            SetupCommands();

            PropertyGrid.PropertyValueChanged += PropertyGridOnPropertyValueChanged;
            RecentFileList.MenuClick += (s, e) => OpenProjectFile(e.Filepath);

            this.SourceInitialized += (x, y) =>
            {
                //LoadAndApplyConfigSettings(); does not work here
            };

        }



        //TODO: This needs to go. Need to redesign the property grid; it fires on every mouse click anywhere!
        private void PropertyGridOnPropertyValueChanged(object sender, PropertyValueChangedEventArgs args)
        {
            if (!ReferenceEquals(sender, PropertyGrid))
                return;

            var a = args;
            var orig = args.OriginalSource;
            Debug.WriteLine($"XXXX {MethodBase.GetCurrentMethod().Name}: {args.OldValue}-->{args.NewValue}");

            if (CurrentDesigner != null)
                CurrentDesigner.IsModified = true;
        }





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            KeyBinding keyBinding = new KeyBinding(ReloadFromFileCmd, Key.F5, ModifierKeys.None);
            InputBindings.Add(keyBinding);
            LoadAndApplyConfigSettings();

            //TODO: Select default tool upon window loaded
            //SelectTool(TOOL_SELECT);
        }



        private void SetupDrawingTools()
        {
            DrawingTools = new ObservableCollection<IDrawingTool>();

            DefaultDrawingTool = RegisterDrawingTool<SelectDrawingTool>();
            RegisterDrawingTool<PanDrawingTool>();
            RegisterDrawingTool<NavigateDrawingTool>();

            RegisterDrawingTool<SeparatorDrawingTool>();

            RegisterDrawingTool<RectangleDrawingTool>();
            RegisterDrawingTool<EllipseDrawingTool>();
            RegisterDrawingTool<TextDrawingTool>();
            RegisterDrawingTool<BoxDrawingTool>();
            RegisterDrawingTool<ImageDrawingTool>();
        }


        #endregion


        #region Config

        private void LoadAndApplyConfigSettings()
        {
            if (File.Exists(ConfigFileName))
            {
                string configStr = File.ReadAllText(ConfigFileName);
                Config = JsonConvert.DeserializeObject<ConfigSettings>(configStr);
            }
            else
            {
                Config = new ConfigSettings();
            }
            Debug.Assert(Config != null);
            ApplyConfigSettings();
        }



        private void PersistConfigSettings()
        {
            SaveConfigSettings();
            string json = JsonConvert.SerializeObject(Config, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
            string path = System.IO.Path.GetDirectoryName(ConfigFileName);
            if (path != null)
            {
                Directory.CreateDirectory(path);
                File.WriteAllText(ConfigFileName, json);
            }
        }



        private void ApplyConfigSettings()
        {
            //TODO: This doesn't actually work

            if (Config.Maximized)
                this.WindowState = WindowState.Maximized;
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = double.IsNaN(Config.Left) ? Config.Left: this.Left;
                this.Top = double.IsNaN(Config.Top) ? Config.Top : this.Top;
                this.Width = double.IsNaN(Config.Width) ? Config.Width : this.Width;
                this.Height = double.IsNaN(Config.Height) ? Config.Height : this.Height;
            }
        }


        private void SaveConfigSettings()
        {
            Config.Maximized = this.WindowState == WindowState.Maximized;
            if (!Config.Maximized)
            {
                Config.Left = this.Left;
                Config.Top = this.Top;
                Config.Width = this.Width;
                Config.Height = this.Height;
            }

        }



        #endregion



        #region General UI Event Handling


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            PersistConfigSettings();
            if (SomePageNeedsSaving())
            {
                MessageBoxResult result = MessageBox.Show("Save changes to the project?", "Project was modified!", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    SaveModifiedPages();
                else if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }


        private void DesignerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //DesignerBorder.Width = DesignerCanvas.Width + 4;
            //DesignerBorder.Height = DesignerCanvas.Height + 4;
        }



        private void ListBoxPages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxPages.SelectedIndex < 0)
                return;
            var href = ListBoxPages.Items[ListBoxPages.SelectedIndex] as string;
            NewPageTab(href, Engine.Project.Background, OnNavigatorLoaded);
        }



        private void ListBoxLayouts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxLayouts.SelectedIndex < 0)
                return;
            var href = ListBoxLayouts.Items[ListBoxLayouts.SelectedIndex] as string;
            NewLayoutTab(href, Engine.Project.Background, OnNavigatorLoaded);
        }


        private void ListBoxParts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxParts.SelectedIndex < 0)
                return;
            var href = ListBoxParts.Items[ListBoxParts.SelectedIndex] as string;
            NewPartTab(href, Engine.Project.Background, OnNavigatorLoaded);
        }


        private void Outline_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = Outline.SelectedItem as UIElementProxy;
            if (selectedItem == null)
                return;
            CurrentDesigner.SelectItem(selectedItem.UIElement);
        }



        private void DocumentPane_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //var s = JsonConvert.SerializeObject(e, Formatting.Indented);

            if (e.PropertyName != nameof(LayoutDocumentPane.SelectedContent))
                return;
            DocumentTabChangeHandler(e);
        }



        private void DocumentTabChangeHandler(PropertyChangedEventArgs e)
        {

            //var docPane = DocumentPane;
            //CurrentDocument = docPane?.SelectedContent?.Content as LayoutDocument;
            //CurrentDesignerIndex = DocumentPane.SelectedContentIndex;

            if (DocumentPane.SelectedContentIndex >= 0)
            {
                CurrentDesigner = DocumentPane.Children[DocumentPane.SelectedContentIndex].Content as DesignerNavigator;
                SelectPageAndTool(CurrentDesigner, true);
            }
            else
            {
                CurrentDesigner = null;
            }
            PopulateOutlineTree();
            Debug.WriteLine($"DocumentPane.{e.PropertyName} changed. CurrentDesigner is now: {CurrentDesigner?.AbsoluteId} {CurrentDesigner?.PageName}");
        }



        private bool suppressPagesChangeNotification = false;

        //TODO: Move somewhere else
        private void SelectPageAndTool(DesignerNavigator designer, bool suppress)
        {
            if (designer == null)
                return;
            suppressPagesChangeNotification = suppress;
            ListBoxPages.SelectedIndex = ListBoxPages.Items.IndexOf(designer.PageName);
            suppressPagesChangeNotification = false;
            Tools.SelectedItem = designer.SelectedDrawingTool;
        }




        private void NavigatorSelectionChangedHandler(DesignerNavigator sender, FrameworkElement element)
        {
            //TODO: This is likely where we can create a wrapper around the element to only show the correct properties.

            OnPropertyChanged(nameof(CurrentElement));
        }


        #endregion



        #region Project-File Operations


        void NewProject()
        {
            CloseProject();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Project File";
            dlg.DefaultExt = Engine.DefaultFileExt;
            dlg.Filter = Engine.DocumentFileFilters;
            if (dlg.ShowDialog() != true)
                return;

            var project = Engine.NewProject(dlg.FileName);
            if (project == null)
                return;

            FileName = dlg.FileName;

            NewPageNode();

            var pages = Engine.Pages;
            Pages = new ObservableCollection<string>(pages);

            var layouts = Engine.Layouts;
            Layouts = new ObservableCollection<string>(layouts);

            var parts = Engine.Parts;
            Parts = new ObservableCollection<string>(parts);

            var images = Engine.Images;
            Images = new ObservableCollection<string>(images);
        }


        void CloseProject()
        {
            var tabs = new List<LayoutContent>(DocumentPane.Children);
            foreach (var tab in tabs)
                tab.Close();
            LayerOutline?.Clear();
            Pages?.Clear();
            Layouts?.Clear();
            Parts?.Clear();
            Images?.Clear();
            Engine?.CloseProject();
            FileName = null;
            //PropertyGrid.SelectedObject = null; NO!
        }



        void OpenProject()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open WireFlow Project";
            dlg.Filter = Engine.DocumentFileFilters;
            if (dlg.ShowDialog() != true)
                return;
            OpenProjectFile(dlg.FileName);
            RecentFileList.InsertFile(dlg.FileName);
        }




        void ReloadProject()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                return;
            //TODO: Return to the current page upon project reload
            //string nodeName = CurrentPageName;
            OpenProjectFile(FileName);
            //if (CurrentPageName != nodeName)
            //{
            //    var result = NavigateToNode(nodeName);
            //    Debug.WriteLine($"Navigation result: {result}");
            //}
        }




        void OpenProjectFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;
            CloseProject();

            var project = Engine.OpenProject(fileName);
            if (project == null)
                return;
            string homePage = project.Home;
            //string background = Project.Background;


            var projectLayouts = Engine.Layouts;
            Layouts = new ObservableCollection<string>(projectLayouts);

            var projectParts = Engine.Parts;
            Parts = new ObservableCollection<string>(projectParts);

            var projectPages = Engine.Pages;
            Pages = new ObservableCollection<string>(projectPages);

            var projectImages = Engine.Images;
            Images = new ObservableCollection<string>(projectImages);


            var index = projectPages.IndexOf(homePage);
            if (index >= 0)
            {
                NewPageTab(homePage, project.Background, OnNavigatorLoaded);
                //CurrentPageName = homePage;
            }

            //if (result == MessageBoxResult.Cancel)
            //    CloseProject();
        }



        public void OnNavigatorLoaded(DesignerNavigator sender)
        {
        }



        void SaveProjectFile()
        {
            if (String.IsNullOrWhiteSpace(FileName))
                SaveProjectFileAs();
            else
                DoSaveProjectFile();
        }



        void SaveProjectFileAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save WireFlow Project As...";
            dlg.Filter = Engine.DocumentFileFilters;
            if (dlg.ShowDialog() != true)
                return;
            SaveProjectFileAs(dlg.FileName);
        }



        void SaveProjectFileAs(string projectFileName)
        {
            //TODO: Save as new project
        }



        void DoSaveProjectFile()
        {
            SaveModifiedPages();
            // Commit changes to the project itself
            Engine.SaveProject();
        }




        void SaveModifiedPages()
        {
            foreach (var tab in DocumentPane.Children)
            {
                var navDoc = tab as DesignerNavigatorDocument;
                if (navDoc == null)
                    continue;
                var nav = navDoc.Navigator;
                nav.SaveCurrentPage();
            }
        }


        private bool SomePageNeedsSaving()
        {
            foreach (LayoutContent child in DocumentPane.Children)
            {
                DesignerNavigatorDocument navDoc = child as DesignerNavigatorDocument;
                if (navDoc != null)
                {
                    var nav = navDoc.Navigator;
                    if (nav.IsModified) return true;
                }
            }
            return false;
        }



        private void ImportImages()
        {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Import an Image";
            dlg.Filter = "All image files (png,jpg,gif,bmp)|*.png;*jpg;*jpeg;*.gif;*.bmp|All files|*";
            dlg.Multiselect = true;
            var userResponse = dlg.ShowDialog();
            if (userResponse != true)
                return;
            Engine.ImportImages(dlg.FileNames);
            var projectImages = Engine.Images;
            Images = new ObservableCollection<string>(projectImages);

        }



        private void DeleteImages()
        {
            var numImages = ListBoxImages.SelectedItems.Count;

            MessageBoxResult result = MessageBox.Show($"Delete {numImages} selected image(s) from the project?", "Delete Images", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            Engine.DeleteImages(ListBoxImages.SelectedItems);
            var projectImages = Engine.Images;
            Images = new ObservableCollection<string>(projectImages);
        }


        #endregion






        #region Page-Node Operations


        private void CreateNewPage(string href = null)
        {
            NewPageDlg dlg = new NewPageDlg(this, Engine, href);
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            Engine.CreatePage(dlg.PageName, dlg.LayoutName);
            var pages = Engine.Pages;
            Pages = new ObservableCollection<string>(pages);
        }



        //private MessageBoxResult OfferToCreateNewPage(string href)
        //{
        //    var msgResult = MessageBox.Show($"The page \"{href}\" does not seem to exist. Create?", "Page Not Found",
        //        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        //    if (msgResult != MessageBoxResult.Yes)
        //        return msgResult;
        //    CreateNewPage(href);
        //    return msgResult;
        //}



        private void NewPageNode(bool addToHistory = true)
        {
            var href = Engine.DefaultIndexNode;
            NewPageTab(href, Engine.Project.Background, OnNavigatorLoaded);
        }



        private void DeleteSelectedPage()
        {
            string layoutName = CurrentPageName;

            var msgResult = MessageBox.Show($"Delete the selected page \"{layoutName}\"?", "Delete Page",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgResult != MessageBoxResult.Yes)
                return;
            Engine.DeletePage(CurrentDesigner.PageName);
            var pages = Engine.Pages;
            Pages = new ObservableCollection<string>(pages);
        }



        #endregion



        #region Layout Operations


        private void CreateNewLayout(string href = null)
        {
            NewPageDlg dlg = new NewPageDlg(this, Engine, href);
            dlg.Title = "New Layout";
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            //    return this.ShowDialogResult(result);
            //Debug.WriteLine($"CREATING LAYOUT: {dlg.PageName}");
            Engine.CreateLayout(dlg.PageName, dlg.LayoutName);
            var layouts = Engine.Layouts;
            Layouts = new ObservableCollection<string>(layouts);
        }




        private void DeleteSelectedLayout()
        {
            string layoutName = CurrentLayoutName;
            var msgResult = MessageBox.Show($"Delete the selected layout \"{layoutName}\"?", "Delete Layout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgResult != MessageBoxResult.Yes)
                return;
            Engine.DeleteLayout(layoutName);
            var layouts = Engine.Layouts;
            Layouts = new ObservableCollection<string>(layouts);
        }

        #endregion


        #region Part Operations


        private void CreateNewPart(string href = null)
        {
            NewPartDlg dlg = new NewPartDlg(this, href);
            dlg.Title = "New Part";
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            Engine.CreatePart(dlg.Name);
            Parts = new ObservableCollection<string>(Engine.Parts);
        }




        private void DeleteSelectedPart()
        {
            string partName = CurrentPartName;
            var msgResult = MessageBox.Show($"Delete the selected part \"{partName}\"?", "Delete Part",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgResult != MessageBoxResult.Yes)
                return;
            Engine.DeleteLayout(partName);
            var parts = Engine.Parts;
            Parts = new ObservableCollection<string>(parts);
        }



        #endregion


        #region Document Pane - Navigation


        private DesignerNavigatorDocument NewPageTab(string href, string background, Action<DesignerNavigator> onLoaded)
        {
            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator);
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            //navigator.NavigationFailed += NavigatorNavigationFailedHandler;
            navigator.PageName = href;
            navigator.SelectedDrawingTool = DefaultDrawingTool;
            DocumentPane.SelectedContentIndex = DocumentPane.Children.Count - 1;
            CurrentDesigner = navigator;

            NavigateToPage(CurrentDesigner, href);

            //Debug.WriteLine($"Designer: {CurrentDesigner.AbsoluteId} PageName: {CurrentDesigner.PageName}-->{value}");
            //TODO: Verify this is not something important
            //OnPropertyChanged(nameof(CurrentPageName));


            navigator.Focus();
            return doc;
        }



        private DesignerNavigatorDocument NewLayoutTab(string href, string background, Action<DesignerNavigator> onLoaded)
        {
            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator);
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            //navigator.NavigationFailed += NavigatorNavigationFailedHandler;
            navigator.PageName = href;
            navigator.SelectedDrawingTool = DefaultDrawingTool;
            DocumentPane.SelectedContentIndex = DocumentPane.Children.Count - 1;
            CurrentDesigner = navigator;

            NavigateToLayout(CurrentDesigner, href);

            navigator.Focus();
            return doc;
        }



        private DesignerNavigatorDocument NewPartTab(string href, string background, Action<DesignerNavigator> onLoaded)
        {
            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator);
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            //navigator.NavigationFailed += NavigatorNavigationFailedHandler;
            navigator.PageName = href;
            navigator.SelectedDrawingTool = DefaultDrawingTool;
            DocumentPane.SelectedContentIndex = DocumentPane.Children.Count - 1;
            CurrentDesigner = navigator;

            NavigateToPart(CurrentDesigner, href);

            navigator.Focus();
            return doc;
        }



        private void NavigatorNavigatingHandler(DesignerNavigator nav, string href)
        {


            //TODO: See if the page already open in another tab, if so switch to it?

            NavigateToPage(nav, href);
        }



        private void ListBoxPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressPagesChangeNotification)
                return;
            if (CurrentDesigner?.Mode != MouseDragMode.Navigate)
                return;
            if (e.AddedItems.Count == 0)
                return;
            var href = (string)e.AddedItems[0];

            NavigateToPage(CurrentDesigner, href);
        }



        private void NavigatorNavigationFailedHandler(DesignerNavigator nav, string href)
        {
            //var result = OfferToCreateNewPage(href);
            //Debug.WriteLine($"OfferToCreateNewPage.result={result}");
        }




        private string navigatingTo = null;

        private void NavigateToPage(DesignerNavigator designer, string href)
        {
            if (navigatingTo != null)
                return;
            navigatingTo = href;
            try
            {
                designer.NavigateToPage(href);
                PopulateOutlineTree(); //TODO!!!! designer
                SelectPageAndTool(designer, false);
            }
            finally
            {
                navigatingTo = null;
            }
        }

        private void NavigateToLayout(DesignerNavigator designer, string href)
        {
            if (navigatingTo != null)
                return;
            navigatingTo = href;
            try
            {
                designer.NavigateToLayout(href);
                PopulateOutlineTree(); //TODO!!!! designer
                SelectPageAndTool(designer, false);
            }
            finally
            {
                navigatingTo = null;
            }
        }



        private void NavigateToPart(DesignerNavigator designer, string href)
        {
            if (navigatingTo != null)
                return;
            navigatingTo = href;
            try
            {
                designer.NavigateToPart(href);
                PopulateOutlineTree(); //TODO!!!! designer
                SelectPageAndTool(designer, false);
            }
            finally
            {
                navigatingTo = null;
            }
        }



        private void ListBoxPages_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            //if (item == null)
            //    return;

            //var block = e.OriginalSource as TextBlock;
            //if (block == null)
            //    return;
            //var href = block.Text;
            //CurrentDesigner.NavigateToPage(href);
        }




        // Arbitrary, blank designer
        private DesignerNavigatorDocument CreateBlankDocumentTab(string title, Action<DesignerNavigator> onLoaded)
        {
            var doc = NewPageTab(null, Constants.DEFAULT_BACKGROUND, onLoaded);
            doc.Title = title;

            PopulateOutlineTree();

            var nav = doc.Navigator;
            var canvas = nav.Designer.Canvas;

            canvas.Width = 1024;
            canvas.Height = 768;
            canvas.Background = ColorUtils.StringToBrush("#777");

            return doc;
        }





        #endregion















        #region Layers Outline


        public void ClearOutlineTree()
        {
            LayerOutline = null;
        }


        public void PopulateOutlineTree()
        {
            if (CurrentDesigner == null)
            {
                ClearOutlineTree();
                return;
            }
            LayerOutline = new ObservableCollection<UIElementProxy>();
            OutlinePopulate(CurrentDesigner.Designer.Canvas);
        }





        private void OutlinePopulate(Panel parent)
        {
            foreach (FrameworkElement element in parent.Children)
            {
                if (element is Layout)
                {
                    Layout layer = element as Layout;
                    var proxy = new UILayoutProxy(element);
                    LayerOutline.Insert(0, proxy);
                    OutlinePopulateLayout(layer, proxy);
                }
                else
                {
                    var proxy = new UIElementProxy(element);
                    LayerOutline.Insert(0, proxy);
                }
            }
            Outline.ExpandAll();
        }


        private void OutlinePopulateLayout(Layout layout, UILayoutProxy layoutProxy)
        {
            foreach (FrameworkElement element in layout.Children)
            {
                if (element is Layer)
                {
                    Layer layer = element as Layer;
                    var proxy = new UILayerProxy(element);
                    layoutProxy.Elements.Insert(0, proxy);
                    OutlinePopulateLayer(layer, proxy);
                }
            }
        }



        private void OutlinePopulateLayer(Layer layer, UILayerProxy layerProxy)
        {
            UIElementProxy proxy;
            foreach (FrameworkElement element in layer.Children)
            {
                if (element is Part)
                    proxy = new UIPartProxy(element);
                else if (element is Group)
                    proxy = new UIGroupProxy(element);
                else if (element is Box)
                    proxy = new UIBoxProxy(element);
                else if (element is ImageBox)
                    proxy = new UIImageProxy(element);
                else if (element is TextBlock)
                    proxy = new UITextProxy(element);
                else
                    proxy = new UIElementProxy(element);
                layerProxy.Elements.Insert(0, proxy);
            }
        }







        // -------------
        //http://stackoverflow.com/questions/3225940/prevent-automatic-horizontal-scroll-in-treeview/34269542#34269542
        //

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true; //prevent event bubbling
            var item = (TreeViewItem)sender;
            TreeView tree = GetParentTree(item) as TreeView;
            if (tree != null)
            {
                var scrollViewer = tree.Template.FindName("_tv_scrollviewer_", tree) as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToLeftEnd();//prevent horizontal scroll
                    Point relativePoint = item.TransformToAncestor(tree).Transform(new Point(0, 0));//get position of a selected item
                    if (relativePoint.Y <= scrollViewer.ContentVerticalOffset) return;//do no scroll if we select inside one 'scroll screen'
                    scrollViewer.ScrollToVerticalOffset(relativePoint.Y);//scroll to Y of a selected item
                }
            }
        }


        private DependencyObject GetParentTree(DependencyObject item)
        {
            var target = VisualTreeHelper.GetParent(item);
            return target is TreeView ? target : GetParentTree(target);
        }


        private void TreeViewItem_Selected(object sender, RoutedEventArgs e) //handle programmatically selected items
        {
            var item = (TreeViewItem)sender;
            item.BringIntoView();
            e.Handled = true;
        }

        // -------------

        #endregion


        #region Drawing Tools


        T RegisterDrawingTool<T>() where T : IDrawingTool, new()
        {
            var tool = new T();
            AddDrawingTool(tool);
            return tool;
        }



        void AddDrawingTool(IDrawingTool tool)
        {
            DrawingTools.Add(tool);
        }


        #endregion




        #region Exporting Page


        class CustomResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty prop = base.CreateProperty(member, memberSerialization);

                var propName = prop.PropertyName.ToLower();

                if (prop.PropertyName == "Children")
                    prop.ShouldSerialize = obj => true;
                else if (propName == "datacontext" || propName == "contextmenu" || propName == "parent" || propName.StartsWith("touches")
                    || propName == "focusvisualstyle")
                    prop.ShouldSerialize = obj => false;
                else if (prop.DeclaringType != typeof(FrameworkElement) &&
                    prop.PropertyType.IsClass &&
                    prop.PropertyType != typeof(string))
                {
                    prop.ShouldSerialize = obj => false;
                }

                return prop;
            }
        }



        public void ExportPageAs()
        {
            var extensions = new[] { ".png", ".jpg", ".json"};

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Export Page Content As...";
            dlg.Filter = "PNG Image|*.png|JPEG image|*.jpg|JSON File|*.json";
            if (dlg.ShowDialog() != true)
                return;
            var fileTypeIndex = dlg.FilterIndex;
            var path = System.IO.Path.GetDirectoryName(dlg.FileName);
            Debug.Assert(path != null);
            string exportFileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
            var ext = extensions[fileTypeIndex - 1];
            var dlgExt = System.IO.Path.GetExtension(dlg.FileName);
            if (string.IsNullOrWhiteSpace(dlgExt))
                dlgExt = ext;
            if (string.IsNullOrWhiteSpace(exportFileName))
            {
                MessageBox.Show("You must supply a file name.", "Unable to Export", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            exportFileName = System.IO.Path.Combine(path, exportFileName + dlgExt);
            switch (fileTypeIndex)
            {
                case 1:
                    ExportDesignerCanvasAsPNG(exportFileName);
                    break;
                case 2:
                    ExportDesignerCanvasAsJPG(exportFileName);
                    break;
                case 3:
                    ExportPageAsJSON(exportFileName);
                    break;
            }

        }



        public void ExportPageAsXAML(string fileName)
        {


        }



        public void ExportPageAsJSON(string fileName)
        {
            string s = JsonConvert.SerializeObject(CurrentDesigner.Designer.Canvas, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new CustomResolver()
            });
            File.WriteAllText(fileName, s);
        }



        private void CopyCanvasToClipboardAsPNG()
        {
            var stream = DesignerCanvasToPNG();


        }




        private void ExportDesignerCanvasAsPNG(string fileName)
        {
            var stream = DesignerCanvasToPNG();
            File.WriteAllBytes(fileName, stream.ToArray());
            stream.Close();
        }


        private void ExportDesignerCanvasAsJPG(string fileName)
        {
            var stream = DesignerCanvasToJPG();
            File.WriteAllBytes(fileName, stream.ToArray());
            stream.Close();
        }




        private MemoryStream DesignerCanvasToPNG()
        {
            Transform rt = CurrentDesigner.RenderTransform;
            try
            {
                CurrentDesigner.RenderTransform = null;
                var designerCanvas = CurrentDesigner.Designer.Canvas;
                BitmapSource rtb = CaptureScreen(designerCanvas,96,96);

                //encode as PNG
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                //save to memory stream
                MemoryStream ms = new MemoryStream();
                pngEncoder.Save(ms);
                return ms;
            }
            finally
            {
                CurrentDesigner.RenderTransform = rt;
            }


        }




        private MemoryStream DesignerCanvasToJPG()
        {
            Transform rt = CurrentDesigner.RenderTransform;
            try
            {
                CurrentDesigner.RenderTransform = null;
                var designerCanvas = CurrentDesigner.Designer.Canvas;
                BitmapSource rtb = CaptureScreen(designerCanvas, 96, 96);

                //encode as JPG
                BitmapEncoder pngEncoder = new JpegBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                //save to memory stream
                MemoryStream ms = new MemoryStream();
                pngEncoder.Save(ms);
                return ms;
            }
            finally
            {
                CurrentDesigner.RenderTransform = rt;
            }
        }





        private static BitmapSource CaptureScreen(Visual target, double dpiX, double dpiY)
        {
            if (target == null)
            {
                return null;
            }
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
                (int)(bounds.Height * dpiY / 96.0),
                dpiX,
                dpiY,
                PixelFormats.Pbgra32);


            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);

            return rtb;
        }

        #endregion






        #region Unused Snippets

        //http://stackoverflow.com/questions/46805/custom-cursor-in-wpf
        //http://stackoverflow.com/a/2837158/858161
        //public Cursor ConvertToCursor(FrameworkElement visual, Point hotSpot)
        //{
        //    int width = (int)visual.Width;
        //    int height = (int)visual.Height;

        //    // Render to a bitmap
        //    var bitmapSource = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        //    bitmapSource.Render(visual);

        //    // Convert to System.Drawing.Bitmap
        //    var pixels = new int[width * height];
        //    bitmapSource.CopyPixels(pixels, width, 0);
        //    var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        //    for (int y = 0; y < height; y++)
        //        for (int x = 0; x < width; x++)
        //            bitmap.SetPixel(x, y, Color.FromArgb(pixels[y * width + x]));

        //    // Save to .ico format
        //    var stream = new MemoryStream();
        //    System.Drawing.Icon.FromHandle(resultBitmap.GetHicon()).Save(stream);

        //    // Convert saved file into .cur format
        //    stream.Seek(2, SeekOrigin.Begin);
        //    stream.WriteByte(2);
        //    stream.Seek(10, SeekOrigin.Begin);
        //    stream.WriteByte((byte)(int)(hotSpot.X * width));
        //    stream.WriteByte((byte)(int)(hotSpot.Y * height));
        //    stream.Seek(0, SeekOrigin.Begin);

        //    // Construct Cursor
        //    return new Cursor(stream);
        //}




        #region XAML File Handling



        void ImportXAMLFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "xaml";
            if (!String.IsNullOrWhiteSpace(FileFolder))
                dlg.InitialDirectory = FileFolder;

            dlg.Filter = "XAML files|*.xaml|All files|*";
            if (dlg.ShowDialog() != true)
                return;
            OpenNamedXAMLFile(dlg.FileName);
        }





        void ExportXAMLFileAs()
        {
            return; //TODO: Change to export as XAML

            //SaveFileDialog dlg = new SaveFileDialog();
            //dlg.DefaultExt = "xaml";
            //dlg.Filter = "XAML files|*.xaml|All files|*";
            //if (dlg.ShowDialog() != true)
            //    return;
            //ExportXAMLFile(dlg.FileName);
        }






        void ReloadXAMLFile()
        {
            return; //TODO: Change


            //if (String.IsNullOrWhiteSpace(FileName))
            //    return;
            //if (MessageBox.Show(
            //    "Are you sure you want to reload project from disk?",
            //    "Confirm Reload",
            //    MessageBoxButton.YesNo,
            //    MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    OpenNamedXAMLFile(FileName);
            //}
        }




        private void OpenNamedXAMLFile(string fileToOpen)
        {
            //try
            //{
            //    DesignerCanvas.Children.Clear();
            //    using (FileStream fs = new FileStream(fileToOpen, FileMode.Open, FileAccess.Read, FileShare.Read))
            //    {
            //        Panel doc = XamlReader.Load(fs) as Panel;
            //        if (doc != null)
            //        {
            //            MoveChildElements(doc, DesignerCanvas);

            //            DesignerCanvas.Background = doc.Background;
            //            DesignerCanvas.Width = doc.Width;
            //            DesignerCanvas.Height = doc.Height;
            //        }
            //    }
            //    FileName = fileToOpen;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(string.Format("Unable to load the specified file: {0}", ex.Message), "Error");
            //}
        }



        void ExportXAMLFile(String fileName)
        {
            //XmlWriterSettings writerSettings = new XmlWriterSettings();
            //writerSettings.Indent = true;
            //writerSettings.CloseOutput = true;
            //XmlWriter xmlWriter = XmlWriter.Create(sw.BaseStream, writerSettings);

            //Transform rt = DesignerCanvas.RenderTransform;
            //try
            //{
            //    DesignerCanvas.RenderTransform = null;
            //    string mystrXAML = XamlWriter.Save(DesignerCanvas);
            //    using (FileStream fs = File.Create(fileName))
            //    {
            //        using (StreamWriter sw = new StreamWriter(fs))
            //        {
            //            sw.Write(mystrXAML);
            //            sw.Close();
            //        }
            //    }
            //    FileName = fileName;
            //    //
            //}
            //finally
            //{
            //    DesignerCanvas.RenderTransform = rt;
            //}
        }


        #endregion





        #endregion



        #region Debugging



        private void DebugGenerateWPFColorChart()
        {
            CreateBlankDocumentTab("WPF Color Chart", null);


            var canvas = CurrentDesigner.Designer.Canvas;

            //DesignerCanvas.Margin = new Thickness(64, 48, 64, 48);

            canvas.Width = 1080;
            canvas.Height = 830;
            canvas.Background = Brushes.White;


            var panel = new WrapPanel();
            panel.Orientation = Orientation.Vertical;
            panel.Width = canvas.Width - 128;
            panel.Height = canvas.Height - 96;
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.Background = Brushes.White;
            panel.ClipToBounds = true;

            Canvas.SetLeft(panel, 64);
            Canvas.SetTop(panel, 76);

            canvas.Children.Add(panel);


            BrushConverter conv = new BrushConverter();
            foreach (PropertyInfo prop in typeof(Colors).GetProperties())
            {
                if (prop.Name.ToLower() == "transparent")
                    continue;

                Brush brush = (Brush)conv.ConvertFromString(prop.Name);

                Rectangle rect = new Rectangle();
                rect.Fill = brush;
                rect.Width = 56;
                rect.Height = 19.25;
                rect.Margin = new Thickness(0.64);

                TextBlock rgb = new TextBlock();
                rgb.Text = brush.ToString().Substring(3);
                rgb.VerticalAlignment = VerticalAlignment.Center;
                rgb.Width = 48;
                rgb.Margin = new Thickness(3, 0, 0, 0);
                rgb.Foreground = Brushes.DarkGray;

                TextBlock text = new TextBlock() { Text = prop.Name };
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Width = 132;
                text.Margin = new Thickness(3, 0, 0, 0);
                text.FontSize += 2;

                Panel subPanel = new WrapPanel();
                subPanel.Children.Add(rect);
                subPanel.Children.Add(rgb);
                subPanel.Children.Add(text);

                panel.Children.Add(subPanel);
            }



            TextBlock title = new TextBlock() { Text = "WPF ALPHABETICAL COLOR CHART" };
            title.TextAlignment = TextAlignment.Center;
            title.VerticalAlignment = VerticalAlignment.Center;
            title.Width = canvas.Width;
            title.FontSize = 24;
            title.FontWeight = FontWeights.ExtraBold;
            title.Foreground = Brushes.LightGray;
            title.Margin = new Thickness(0, 0, 0, 0);
            Canvas.SetTop(title, 36);
            canvas.Children.Add(title);

        }




        //private void DebugGenerateWPFColorChart1()
        //{

        //    var doc = CreateBlankDocumentTab("WPF Color Chart", (s) =>
        //    {
        //        var canvas = s.Designer.Canvas;

        //        Debug.WriteLine($"Canvas: {canvas.GetHashCode()}");
        //        canvas.Width = 1080;
        //        canvas.Height = 830;
        //        canvas.Background = Brushes.White;

        //        var panel = new WrapPanel();
        //        panel.Orientation = Orientation.Vertical;
        //        panel.Width = canvas.Width - 128;
        //        panel.Height = canvas.Height - 96;
        //        panel.VerticalAlignment = VerticalAlignment.Stretch;
        //        panel.HorizontalAlignment = HorizontalAlignment.Stretch;
        //        panel.ClipToBounds = true;

        //        Canvas.SetLeft(panel, 64);
        //        Canvas.SetTop(panel, 76);

        //        canvas.Children.Add(panel);



        //        var brushes = typeof(Brushes).GetProperties().Select(p => new { p.Name, Brush = (Brush)p.GetValue(null, null) }).OrderByDescending(b => b.Brush.ToString().Substring(3));

        //        foreach (var brush in brushes)
        //        {
        //            if (brush.Name.ToLower() == "transparent")
        //                continue;

        //            Rectangle rect = new Rectangle();
        //            rect.Fill = brush.Brush;
        //            rect.Width = 56;
        //            rect.Height = 19.25;
        //            rect.Margin = new Thickness(0.64);

        //            TextBlock rgb = new TextBlock();
        //            rgb.Text = brush.Brush.ToString().Substring(3);
        //            rgb.VerticalAlignment = VerticalAlignment.Center;
        //            rgb.Width = 48;
        //            rgb.Margin = new Thickness(3, 0, 0, 0);
        //            rgb.Foreground = Brushes.DarkGray;

        //            TextBlock text = new TextBlock() { Text = brush.Name };
        //            text.VerticalAlignment = VerticalAlignment.Center;
        //            text.Width = 132;
        //            text.Margin = new Thickness(3, 0, 0, 0);
        //            text.FontSize += 2;

        //            Panel subPanel = new WrapPanel();
        //            subPanel.Children.Add(rect);
        //            subPanel.Children.Add(rgb);
        //            subPanel.Children.Add(text);

        //            panel.Children.Add(subPanel);
        //        }



        //        TextBlock title = new TextBlock() { Text = "WPF COLOR CHART" };
        //        title.TextAlignment = TextAlignment.Center;
        //        title.VerticalAlignment = VerticalAlignment.Center;
        //        title.Width = canvas.Width;
        //        title.FontSize = 24;
        //        title.FontWeight = FontWeights.ExtraBold;
        //        title.Foreground = Brushes.LightGray;
        //        title.Margin = new Thickness(0, 0, 0, 0);
        //        Canvas.SetTop(title, 36);
        //        canvas.Children.Add(title);


        //        PopulateOutlineTree();
        //    });
        //}



        void DebugBreakpoint([CallerMemberName] string callerName = null)
        {
            Debug.WriteLine($"Breakpoint invoked from {callerName}.");
            if (Debugger.IsAttached)
                Debugger.Break();
        }




        private void DebugCreateBlankCanvas()
        {
            CreateBlankDocumentTab("Blank", (s) =>
            {
                var canvas = s.Designer.Canvas;
                canvas.Width = 1024;
                canvas.Height = 768;
                canvas.Background = Brushes.White;
            });
        }




        void DebugSaveXamlAsJson()
        {
            return;

            //SaveFileDialog dlg = new SaveFileDialog();
            //dlg.Title = "Save Designer XAML as JSON";
            //dlg.DefaultExt = "jason";
            //dlg.Filter = "JSON files|*.json|All files|*";
            //if (dlg.ShowDialog() != true)
            //    return;
            ////Does not work:
            //string s = JsonConvert.SerializeObject(DesignerCanvas, Newtonsoft.Json.Formatting.Indented,
            //    new JsonSerializerSettings
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //        PreserveReferencesHandling = PreserveReferencesHandling.None
            //    });
            //File.WriteAllText(dlg.FileName, s);
        }





        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(name);
                handler(this, e);
            }
        }

        #endregion


        #region Fields

        private string fileName;

        private ObservableCollection<string> pages;
        private ObservableCollection<string> layouts;
        private ObservableCollection<string> parts;
        private ObservableCollection<string> images;

        private ObservableCollection<IDrawingTool> drawingTools;
        private ObservableCollection<UIElementProxy> layerOutline;



        private int selectedToolIndex;
        private int currentDesignerIndex;
        private DesignerNavigator currentDesigner;


        #endregion



        #region Commands

        public ICommand NewFileCmd { get; set; }
        public ICommand OpenFileCmd { get; set; }
        public ICommand SaveFileAsCmd { get; set; }
        public ICommand ExportPageAsCmd { get; set; }
        public ICommand SaveFileCmd { get; set; }
        public ICommand ReloadFromFileCmd { get; set; }
        public ICommand CloseFileCmd { get; set; }

        public ICommand ObjectDuplicateCmd { get; set; }
        public ICommand ObjectDeleteCmd { get; set; }
        public ICommand ObjectBringForwardCmd { get; set; }
        public ICommand ObjectSendBackwardCmd { get; set; }
        public ICommand ObjectBringToFrontCmd { get; set; }
        public ICommand ObjectSendToBackCmd { get; set; }
        public ICommand GroupSelectedCmd { get; set; }
        public ICommand UngroupSelectedCmd { get; set; }

        public ICommand AlignTopsCmd { get; set; }
        public ICommand AlignBottomsCmd { get; set; }
        public ICommand AlignLeftsCmd { get; set; }
        public ICommand AlignRightsCmd { get; set; }
        public ICommand AlignCentersVerticallyCmd { get; set; }
        public ICommand AlignCentersHorizontallyCmd { get; set; }
        public ICommand CenterInPageCmd { get; set; }

        public ICommand MakeSameWidthCmd { get; set; }
        public ICommand MakeSameHeightCmd { get; set; }
        public ICommand MakeSameSizeCmd { get; set; }
        public ICommand ExchangeWidthAndHeightCmd { get; set; }
        public ICommand DistributeHorizontallyCmd { get; set; }
        public ICommand DistributeVerticallyCmd { get; set; }
        public ICommand StackHorizontallyCmd { get; set; }
        public ICommand StackVerticallyCmd { get; set; }


        public ICommand MoveSelectionUpCmd { get; set; }
        public ICommand MoveSelectionDnCmd { get; set; }
        public ICommand MoveSelectionLeftCmd { get; set; }
        public ICommand MoveSelectionRightCmd { get; set; }

        public ICommand NewPageCmd { get; set; }
        public ICommand DeletePageCmd { get; set; }
        public ICommand NewLayoutCmd { get; set; }
        public ICommand DeleteLayoutCmd { get; set; }
        public ICommand NewPartCmd { get; set; }
        public ICommand DeletePartCmd { get; set; }

        public ICommand ConvertToBoxCmd { get; set; }


        public ICommand PagesRenamePageCmd { get; set; }


        public ICommand ImportImageCmd { get; set; }
        public ICommand DeleteImageCmd { get; set; }


        public ICommand ExitCmd { get; set; }


        public ICommand DebugBreakpointCmd { get; set; }
        public ICommand DebugBlankCanvasCmd { get; set; }
        public ICommand DebugGenerateWPFColorChartCmd { get; set; }
        public ICommand DebugSaveXamlAsJsonCmd { get; set; }
        public ICommand DebugPopulateLayersCmd { get; set; }





        private void SetupCommands()
        {
            NewFileCmd = new RelayCommand(o => NewProject());
            OpenFileCmd = new RelayCommand(o => OpenProject());
            SaveFileAsCmd = new RelayCommand(o => SaveProjectFileAs(), o => !string.IsNullOrWhiteSpace(FileName));
            ExportPageAsCmd = new RelayCommand(o => ExportPageAs(), o => CurrentCanvas != null);
            SaveFileCmd = new RelayCommand(o => SaveProjectFile(), o => !string.IsNullOrWhiteSpace(FileName));
            ReloadFromFileCmd = new RelayCommand(o => ReloadProject(), o => !string.IsNullOrWhiteSpace(FileName));
            CloseFileCmd = new RelayCommand(o => CloseProject(), o => !string.IsNullOrWhiteSpace(FileName));

            ObjectDuplicateCmd = new RelayCommand(o => CurrentDesigner?.DuplicateSelectedElements(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ObjectDeleteCmd = new RelayCommand(o =>CurrentDesigner?.DeleteSelectedElements(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ObjectBringForwardCmd = new RelayCommand(o => CurrentDesigner?.ObjectBringForward(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ObjectSendBackwardCmd = new RelayCommand(o => CurrentDesigner?.ObjectSendBackward(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ObjectBringToFrontCmd = new RelayCommand(o => CurrentDesigner?.ObjectBringToFront(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ObjectSendToBackCmd = new RelayCommand(o => CurrentDesigner?.ObjectSendToBack(), o => CurrentDesigner?.HasElementsSelected ?? false);
            GroupSelectedCmd = new RelayCommand(o => CurrentDesigner?.GroupSelected(), o => CurrentDesigner?.CanGroup ?? false);
            UngroupSelectedCmd = new RelayCommand(o => CurrentDesigner?.UngroupSelected(), o => CurrentDesigner?.CanUngroup ?? false);

            AlignTopsCmd = new RelayCommand(o => CurrentDesigner?.AlignTops(), o => CurrentDesigner?.HasElementsSelected ?? false);
            AlignBottomsCmd = new RelayCommand(o => CurrentDesigner?.AlignBottoms(), o => CurrentDesigner?.HasElementsSelected ?? false);
            AlignLeftsCmd = new RelayCommand(o => CurrentDesigner?.AlignLefts(), o => CurrentDesigner?.HasElementsSelected ?? false);
            AlignRightsCmd = new RelayCommand(o => CurrentDesigner?.AlignRights(), o => CurrentDesigner?.HasElementsSelected ?? false);
            AlignCentersVerticallyCmd = new RelayCommand(o => CurrentDesigner?.AlignCentersVertically(), o => CurrentDesigner?.HasElementsSelected ?? false);
            AlignCentersHorizontallyCmd = new RelayCommand(o => CurrentDesigner?.AlignCentersHorizontally(), o => CurrentDesigner?.HasElementsSelected ?? false);
            CenterInPageCmd = new RelayCommand(o => CurrentDesigner?.CenterInPage(), o => CurrentDesigner?.HasElementsSelected ?? false);

            MakeSameWidthCmd = new RelayCommand(o => CurrentDesigner?.MakeSameWidth(), o => CurrentDesigner?.HasElementsSelected ?? false);
            MakeSameHeightCmd = new RelayCommand(o => CurrentDesigner?.MakeSameHeight(), o => CurrentDesigner?.HasElementsSelected ?? false);
            MakeSameSizeCmd = new RelayCommand(o => CurrentDesigner?.MakeSameSize(), o => CurrentDesigner?.HasElementsSelected ?? false);
            ExchangeWidthAndHeightCmd = new RelayCommand(o => CurrentDesigner?.ExchangeWidthAndHeight(), o => CurrentDesigner?.HasElementsSelected ?? false);

            DistributeHorizontallyCmd = new RelayCommand(o => CurrentDesigner?.DistributeHorizontally(), o => CurrentDesigner?.Has3PlusElementsSelected ?? false);
            DistributeVerticallyCmd = new RelayCommand(o => CurrentDesigner?.DistributeVertically(), o => CurrentDesigner?.Has3PlusElementsSelected ?? false);
            StackHorizontallyCmd = new RelayCommand(o => CurrentDesigner?.StackHorizontally(), o => CurrentDesigner?.Has2PlusElementsSelected ?? false);
            StackVerticallyCmd = new RelayCommand(o => CurrentDesigner?.StackVertically(), o => CurrentDesigner?.Has2PlusElementsSelected ?? false);


            MoveSelectionUpCmd = new RelayCommand(o => CurrentDesigner?.MoveSelectionUp(), o => CurrentDesigner?.HasElementsSelected ?? false);
            MoveSelectionDnCmd = new RelayCommand(o => CurrentDesigner?.MoveSelectionDn(), o => CurrentDesigner?.HasElementsSelected ?? false);
            MoveSelectionLeftCmd = new RelayCommand(o => CurrentDesigner?.MoveSelectionLeft(), o => CurrentDesigner?.HasElementsSelected ?? false);
            MoveSelectionRightCmd = new RelayCommand(o => CurrentDesigner?.MoveSelectionRight(), o => CurrentDesigner?.HasElementsSelected ?? false);

            NewPageCmd = new RelayCommand(o => CreateNewPage(), o => IsProjectFileOpen);
            DeletePageCmd = new RelayCommand(o => DeleteSelectedPage(), o => IsPageSelected);
            NewLayoutCmd = new RelayCommand(o => CreateNewLayout(), o => IsProjectFileOpen);
            DeleteLayoutCmd = new RelayCommand(o => DeleteSelectedLayout(), o => IsPageSelected);
            NewPartCmd = new RelayCommand(o => CreateNewPart(), o => IsProjectFileOpen);
            DeletePartCmd = new RelayCommand(o => DeleteSelectedPart(), o => IsPageSelected);

            ConvertToBoxCmd = new RelayCommand(o => ConvertToBox(), o => CurrentDesigner?.HasElementsSelected ?? false);

            ImportImageCmd = new RelayCommand(o => ImportImages());
            DeleteImageCmd = new RelayCommand(o => DeleteImages());


            PagesRenamePageCmd = new RelayCommand(o => InPlaceRenamePage(), o => ListBoxPages.SelectedIndex >= 0);

            ExitCmd = new RelayCommand(o => Close());

            DebugBreakpointCmd = new RelayCommand(o => DebugBreakpoint());
            DebugBlankCanvasCmd = new RelayCommand(o => DebugCreateBlankCanvas());
            DebugGenerateWPFColorChartCmd = new RelayCommand(o => DebugGenerateWPFColorChart());
            DebugSaveXamlAsJsonCmd = new RelayCommand(o => DebugSaveXamlAsJson());
            DebugPopulateLayersCmd = new RelayCommand(o => PopulateOutlineTree());
        }







        private void InPlaceRenamePage()
        {
            if (ListBoxPages.SelectedIndex < 0)
                return;

            var item = ListBoxPages.SelectedItem;
            var href = ListBoxPages.Items[ListBoxPages.SelectedIndex] as string;


            ListBoxPages.BeginEdit((oldName, newName) =>
            {
                Engine.RenamePage(oldName, newName);
                Pages[ListBoxPages.SelectedIndex] = newName;
            });

        }





        private void ConvertToBox()
        {

        }


        #endregion


        #region Constants

        private const string APP_NAME = "WireFlow Designer";


        private const int TOOL_SELECT = 0;
        private const int TOOL_PAN = 1;
        private const int TOOL_NAVIGATE = 2;
        private const int TOOL_RECTANGLE = 4;
        private const int TOOL_ELLIPSE = 5;
        private const int TOOL_BOX = 6;
        private const int TOOL_IMAGE = 7;











        #endregion


    }



    public static class ItemsControlExt
    {

        public static object GetObjectAtPoint<ItemContainer>(this ItemsControl control, Point p)
            where ItemContainer : DependencyObject
        {
            // ItemContainer - can be ListViewItem, or TreeViewItem and so on(depends on control)
            ItemContainer obj = GetContainerAtPoint<ItemContainer>(control, p);
            if (obj == null)
                return null;

            return control.ItemContainerGenerator.ItemFromContainer(obj);
        }

        public static ItemContainer GetContainerAtPoint<ItemContainer>(this ItemsControl control, Point p)
            where ItemContainer : DependencyObject
        {
            HitTestResult result = VisualTreeHelper.HitTest(control, p);
            DependencyObject obj = result.VisualHit;

            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ItemContainer))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            // Will return null if not found
            return obj as ItemContainer;
        }

    }


}
