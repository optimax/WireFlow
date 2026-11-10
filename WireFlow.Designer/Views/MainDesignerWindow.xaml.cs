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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Newtonsoft.Json.Serialization;
using WireFlow.Views;
using OptimaxTools.Dock.Layout;
using OptimaxTools.Wpf;
using MaxServices.Core.Themes;
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
                // An asterisk while a page, layout or part open has changes not saved, as MaxPad marks its document
                string modified = SomePageNeedsSaving() ? "*" : "";
                return string.Format($"[{System.IO.Path.GetFileNameWithoutExtension(FileName)}]{modified} {APP_NAME} - {System.IO.Path.GetDirectoryName(FileName)}");
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
                ShowModified();
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
                // The ListBox reports -1 when its selection is cleared; keep the designer's current tool then.
                if (CurrentDesigner != null && selectedToolIndex >= 0 && selectedToolIndex < Tools.Items.Count)
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


        /// <summary>What Properties shows: the DOM node of the element selected, or of the page when none is.</summary>
        public object CurrentProperties
        {
            get
            {
                var element = CurrentElement;
                if (element == null || CurrentDesigner == null)
                    return null;
                // Several elements selected: what they have in common
                var selected = CurrentDesigner.SelectedElements.Select(FlowView.GetNode).Where(n => n != null).Distinct().ToList();
                if (selected.Count > 1)
                    return new NodeProperties(selected, Engine);
                var node = FlowView.GetNode(element) ?? (element == CurrentCanvas ? CurrentDesigner.CurrentPage : null);
                return node == null ? null : new NodeProperties(node, Engine);
            }
        }






        #endregion




        #region C'tors

        public MainDesignerWindow(WireFlowEngine engine)
        {
            // The theme before the window is built; Window_Loaded applies the rest of the settings
            LoadConfigSettings();
            DesignerTheme.Apply(DesignerTheme.Parse(Config.Theme));
            InitializeComponent();
            ApplyConfigSettings();
            // An ID typed in Properties that another node in the project has is refused
            NodeProperties.IdTaken = (node, id) => CurrentDesigner?.IsIdTaken(node, id) == true;
            // Pages are put in order by dragging them, as by the Move Up and Move Down buttons
            new ListDragReorder(ListBoxPages, MovePage);
            UpdateThemeMenu();
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
            foreach (var list in CompactLists)
            {
                var compacted = list;
                compacted.ItemContainerGenerator.StatusChanged += (s, e) => CompactRows(compacted);
            }
            AppTheme.Changed += (s, e) =>
            {
                foreach (var list in CompactLists)
                    CompactRows(list);
            };
            RecentFileList.MenuClick += (s, e) => OpenProjectFile(e.Filepath);

            this.SourceInitialized += (x, y) =>
            {
                //LoadAndApplyConfigSettings(); does not work here
                AppTheme.ApplyTitleBar(this);
            };

        }


        private static readonly Thickness CompactItemPadding = new Thickness(8, 3, 8, 3);

        // The lists whose rows are as compact as menu items: Tools, Pages, Layouts, Parts, Images and History
        private ListBox[] CompactLists => new ListBox[] { Tools, ListBoxPages, ListBoxLayouts, ListBoxParts, ListBoxImages, ListBoxHistory };

        /// <summary>
        /// Makes a list's rows as compact as menu items. Fluent pads list items by 12 px, which makes a row 45 px tall; the
        /// local padding makes it 27 px, as a Fluent menu item. Classic pads them by 4,1 px, already compact, so it keeps its own.
        /// A local value overrides the theme's padding but keeps its template. It runs as the list makes its rows (for new
        /// items, and as it scrolls) and when the theme changes.
        /// </summary>
        private static void CompactRows(ListBox list)
        {
            if (list.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return;
            bool classic = AppTheme.Choice == ThemeChoice.Classic;
            foreach (var item in list.Items)
                if (list.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem container)
                {
                    if (classic)
                        container.ClearValue(PaddingProperty);
                    else
                        container.Padding = CompactItemPadding;
                }
        }


        // Raised for a value set in the grid only, not for one changed elsewhere, such as by moving the element
        private void PropertyGridOnPropertyValueChanged(object sender, PropertyValueChangedEventArgs args)
        {
            Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}: {args.Item.Name} {args.OldValue}-->{args.NewValue}");

            if (CurrentDesigner != null)
                CurrentDesigner.IsModified = true;
        }





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            KeyBinding keyBinding = new KeyBinding(ReloadFromFileCmd, Key.F5, ModifierKeys.None);
            InputBindings.Add(keyBinding);
            // File > Save
            InputBindings.Add(new KeyBinding(SaveFileCmd, Key.S, ModifierKeys.Control));
            // Edit > Undo, Redo; a text box with the keyboard undoes its own typing first, as its own bindings come before the window's
            InputBindings.Add(new KeyBinding(UndoCmd, Key.Z, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(RedoCmd, Key.Y, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(RedoCmd, Key.Z, ModifierKeys.Control | ModifierKeys.Shift));
            // Back to the Select tool, wherever the keyboard is; the designer's own Esc also ends a drag and clears the selection
            InputBindings.Add(new KeyBinding(new RelayCommand(o => SelectDefaultTool()), Key.Escape, ModifierKeys.None));
            // The panes as they were, before any tab opens: tabs are not kept in the layout
            RestoreDockLayout();
            // Once the window shows, so that it is there while the project loads and behind any message about it
            Dispatcher.BeginInvoke(new Action(ReopenPreviousProject), System.Windows.Threading.DispatcherPriority.ContextIdle);

            //TODO: Select default tool upon window loaded
            //SelectTool(TOOL_SELECT);
        }



        // Tools > Options > Files: the project open when the Designer was last closed, if it is still there, with the tabs
        // that were open, those whose page, layout or part is still there, and the one in front; without any, its home page
        private void ReopenPreviousProject()
        {
            if (!Config.ReopenPreviousProject || !string.IsNullOrWhiteSpace(FileName))
                return;
            string previous = Config.PreviousProject;
            if (string.IsNullOrWhiteSpace(previous) || !File.Exists(previous))
                return;
            var tabs = (Config.PreviousTabs ?? new List<TabSetting>()).Where(t => t != null).ToList();
            int front = Config.PreviousTab;
            OpenProjectFile(previous, openHomePage: false);
            if (string.IsNullOrWhiteSpace(FileName))
                return;
            DesignerNavigatorDocument frontDoc = null, firstDoc = null;
            for (int i = 0; i < tabs.Count; i++)
            {
                var doc = OpenTab(tabs[i].Kind, tabs[i].Name);
                firstDoc ??= doc;
                if (i == front)
                    frontDoc = doc;
            }
            if (firstDoc == null)
            {
                OpenTab(DesignerNavigatorDocument.PageKind, Engine.HomePage);
                return;
            }
            if (frontDoc != null)
                ShowTab(frontDoc);
        }



        // A tab for a page, layout or part of the project, if the project has it (names compare ignoring case)
        private DesignerNavigatorDocument OpenTab(string kind, string name)
        {
            bool Has(IEnumerable<string> names) => !string.IsNullOrWhiteSpace(name) && names != null && names.Contains(name, StringComparer.OrdinalIgnoreCase);
            switch (kind)
            {
                case DesignerNavigatorDocument.PageKind:
                    return Has(Pages) ? NewPageTab(name, Engine.Project.Background, OnNavigatorLoaded) : null;
                case DesignerNavigatorDocument.LayoutKind:
                    return Has(Layouts) ? NewLayoutTab(name, Engine.Project.Background, OnNavigatorLoaded) : null;
                case DesignerNavigatorDocument.PartKind:
                    return Has(Parts) ? NewPartTab(name, Engine.Project.Background, OnNavigatorLoaded) : null;
                default:
                    return null;
            }
        }



        // Esc: the Select tool, in the Tools list and in the current designer
        private void SelectDefaultTool()
        {
            int index = Tools.Items.IndexOf(DefaultDrawingTool);
            if (index >= 0)
                SelectedToolIndex = index;
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

            RegisterDrawingTool<SeparatorDrawingTool>();

            RegisterDrawingTool<StackPanelDrawingTool>();
        }


        #endregion


        #region Config

        private void LoadConfigSettings()
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
            // The color dialog keeps its recent colors in the settings' list, which is saved with them
            Config.RecentColors ??= new List<string>();
            ColorPickerDialog.RecentColors = Config.RecentColors;
        }



        // View → Theme: System, Light, Dark or Classic, as in PDFTools; kept in the settings at once
        private void ThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is ThemeChoice choice))
                return;
            DesignerTheme.Apply(choice);
            Config.Theme = choice.ToString();
            PersistConfigSettings();
            UpdateThemeMenu();
        }



        private void UpdateThemeMenu()
        {
            foreach (var item in ThemeMenuItem.Items.OfType<MenuItem>())
                item.IsChecked = item.Tag is ThemeChoice choice && choice == AppTheme.Choice;
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



        // The window where it was, at its size, maximized or not; a place no longer on any screen (a screen taken away) is left to Windows
        private void ApplyConfigSettings()
        {
            if (!double.IsNaN(Config.Width) && !double.IsNaN(Config.Height) && Config.Width > 100 && Config.Height > 100)
            {
                Width = Config.Width;
                Height = Config.Height;
            }
            if (!double.IsNaN(Config.Left) && !double.IsNaN(Config.Top))
            {
                var visible = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
                var bounds = new Rect(Config.Left, Config.Top, Width, Height);
                if (visible.IntersectsWith(bounds) && visible.Contains(new Point(Config.Left + Math.Min(100, Width / 2), Config.Top + 10)))
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Left = Config.Left;
                    Top = Config.Top;
                }
            }
            if (Config.Maximized)
                WindowState = WindowState.Maximized;
        }


        // The panes as the Designer was last closed: docked, floating or hidden, and their sizes. The layout read replaces the
        // layout objects, so the document pane is found again; a layout without one is not used.
        private void RestoreDockLayout()
        {
            if (string.IsNullOrWhiteSpace(Config.DockLayout))
                return;
            string builtIn = SerializeDockLayout();
            try
            {
                DeserializeDockLayout(Config.DockLayout);
                if (FindDocumentPane() == null && builtIn != null)
                    DeserializeDockLayout(builtIn);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"The docking layout could not be restored: {ex.Message}");
                if (builtIn != null)
                    DeserializeDockLayout(builtIn);
            }
            var pane = FindDocumentPane();
            if (pane != null && pane != DocumentPane)
            {
                DocumentPane.PropertyChanged -= DocumentPane_PropertyChanged;
                DocumentPane = pane;
                DocumentPane.PropertyChanged += DocumentPane_PropertyChanged;
            }
        }


        // The panes in the order the View menu lists them; a pane not named here comes after them
        private static readonly string[] PaneOrder = { "tools", "pages", "layouts", "parts", "images", "history", "outline", "properties" };


        // An item per pane, checked while it shows, made each time the menu opens: restoring a saved layout makes the panes again
        private void ViewMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != ViewMenuItem || DockingManager.Layout == null)
                return;
            foreach (var old in ViewMenuItem.Items.OfType<MenuItem>().Where(m => m.Tag is LayoutAnchorable).ToList())
                ViewMenuItem.Items.Remove(old);
            var panes = DockingManager.Layout.Descendents().OfType<LayoutAnchorable>().Concat(DockingManager.Layout.Hidden).Distinct()
                .OrderBy(p => Array.IndexOf(PaneOrder, p.ContentId) is int i && i >= 0 ? i : int.MaxValue).ToList();
            foreach (var pane in panes)
            {
                var item = new MenuItem { Header = pane.Title, IsCheckable = true, IsChecked = pane.IsVisible, Tag = pane };
                item.Click += (s, a) =>
                {
                    if (pane.IsVisible)
                        pane.Hide();
                    else
                        pane.Show();
                };
                ViewMenuItem.Items.Add(item);
            }
        }


        private LayoutDocumentPane FindDocumentPane() => DockingManager.Layout?.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();


        private string SerializeDockLayout()
        {
            try
            {
                using (var writer = new StringWriter())
                {
                    new DockLayoutSerializer(DockingManager).Serialize(writer);
                    return writer.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"The docking layout could not be kept: {ex.Message}");
                return null;
            }
        }


        private void DeserializeDockLayout(string xml)
        {
            using (var reader = new StringReader(xml))
                new DockLayoutSerializer(DockingManager).Deserialize(reader);
        }


        private void SaveConfigSettings()
        {
            Config.Maximized = this.WindowState == WindowState.Maximized;
            // Maximized or minimized, the place it goes back to
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            if (!bounds.IsEmpty)
            {
                Config.Left = bounds.Left;
                Config.Top = bounds.Top;
                Config.Width = bounds.Width;
                Config.Height = bounds.Height;
            }
            Config.DockLayout = SerializeDockLayout() ?? Config.DockLayout;
            // The project to reopen at startup, when Tools > Options says so, and its tabs in the main pane
            Config.PreviousProject = string.IsNullOrWhiteSpace(FileName) ? null : FileName;
            var tabs = Config.PreviousProject == null ? new List<DesignerNavigatorDocument>()
                : DocumentPane.Children.OfType<DesignerNavigatorDocument>().Where(d => d.Kind != null && !string.IsNullOrWhiteSpace(d.Navigator?.PageName)).ToList();
            Config.PreviousTabs = tabs.Select(d => new TabSetting { Kind = d.Kind, Name = d.Navigator.PageName }).ToList();
            Config.PreviousTab = tabs.IndexOf(DocumentPane.SelectedContent as DesignerNavigatorDocument);
        }



        // Tools > Options...: the dialog shows the settings as they are; OK applies and saves them
        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DesignerOptionsDialog(Config) { Owner = this };
            dialog.ShowDialog();
            if (dialog.Answer != OptimaxTools.Options.OptionsAnswer.Ok)
                return;
            Config.ReopenPreviousProject = dialog.ReopenPreviousProject;
            PersistConfigSettings();
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


        // The layer chosen in the Outline, when it is one
        private Layer OutlineLayer => (Outline.SelectedItem as UILayerProxy)?.UIElement as Layer;


        private bool CanOutlineDelete() =>
            CurrentDesigner != null && (CurrentDesigner.HasElementsSelected || (OutlineLayer != null && (CurrentDesigner.CurrentPage as FlowPage)?.Layers.Count > 1));


        private void OutlineDelete()
        {
            if (CurrentDesigner == null)
                return;
            if (CurrentDesigner.HasElementsSelected)
                CurrentDesigner.DeleteSelectedElements();
            else if (OutlineLayer is Layer layer)
                CurrentDesigner.DeleteLayer(layer);
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
            OnPropertyChanged(nameof(CurrentProperties));
            if (sender == CurrentDesigner)
                UpdateOutlineHighlight();
        }


        /// <summary>Highlights in the Outline the elements selected in the current designer.</summary>
        private void UpdateOutlineHighlight()
        {
            var selected = CurrentDesigner?.SelectedElements;
            foreach (var proxy in OutlineProxies(LayerOutline))
                proxy.IsHighlighted = selected != null && selected.Contains(proxy.UIElement);
        }


        // The Outline's entries and theirs, depth first
        private static IEnumerable<UIElementProxy> OutlineProxies(IEnumerable<UIElementProxy> proxies)
        {
            if (proxies == null)
                yield break;
            foreach (var proxy in proxies)
            {
                yield return proxy;
                if (proxy is UICanvasProxy canvas)
                    foreach (var child in OutlineProxies(canvas.Elements))
                        yield return child;
            }
        }


        private void NavigatorElementsChangedHandler(DesignerNavigator sender)
        {
            // The outline shows only the current designer's elements
            if (sender == CurrentDesigner)
                PopulateOutlineTree();
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

            // The repository may have given the name its extension
            FileName = Engine.ProjectFileName;

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
            // The .wf file that a folder project (.project) was converted into
            if (!string.IsNullOrWhiteSpace(FileName))
                RecentFileList.InsertFile(FileName);
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




        // openHomePage: false when the caller opens the tabs itself (ReopenPreviousProject)
        void OpenProjectFile(string fileName, bool openHomePage = true)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;
            CloseProject();

            FlowProject project;
            string reason;
            try
            {
                project = Engine.OpenProject(fileName);
                reason = "It is not a WireFlow project, or it was made by another version of WireFlow.";
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
            {
                // Converting a folder project (.project) into a .wf file failed
                project = null;
                reason = ex.Message;
            }
            if (project == null)
            {
                MessageBox.Show($"WireFlow Designer could not open \"{fileName}\".\n\n{reason}", "Open Project",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // The first page in the order is where the project starts
            string homePage = Engine.HomePage;
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
            if (openHomePage && index >= 0)
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


        /// <summary>The status bar's word for the page, layout or part in front: Modified while it has changes not saved, as MaxPad's.</summary>
        public string DocumentState => CurrentDesigner == null ? "" : CurrentDesigner.IsModified ? "Modified" : "Saved";


        private void NavigatorModifiedChangedHandler(DesignerNavigator sender) => ShowModified();


        // The caption's asterisk and the status bar follow the documents' changes not saved
        private void ShowModified()
        {
            OnPropertyChanged(nameof(DesignerTitle));
            OnPropertyChanged(nameof(DocumentState));
        }


        private bool SomePageNeedsSaving()
        {
            if (DocumentPane == null)
                return false;
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



        /// <summary>Brings the Images pane up to date with the project's images, as after the image dialog imported or deleted some.</summary>
        public void ReloadImages()
        {
            if (Engine != null)
                Images = new ObservableCollection<string>(Engine.Images);
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



        // Moves a page to an index in the order (the first is where the project starts), keeping it selected
        private void MovePage(string pageName, int index)
        {
            if (!Engine.MovePage(pageName, index))
                return;
            int from = Pages.IndexOf(pageName);
            int to = Engine.Pages.IndexOf(pageName);
            if (from < 0 || to < 0)
            {
                Pages = new ObservableCollection<string>(Engine.Pages);
                return;
            }
            // The page selected stays so: opening it on a change of selection is not wanted here
            suppressPagesChangeNotification = true;
            try
            {
                Pages.Move(from, to);
                ListBoxPages.SelectedItem = pageName;
            }
            finally
            {
                suppressPagesChangeNotification = false;
            }
            ListBoxPages.ScrollIntoView(pageName);
        }


        private int SelectedPageIndex => ListBoxPages.SelectedItem is string name ? Pages?.IndexOf(name) ?? -1 : -1;


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
            Engine.DeletePart(partName);
            var parts = Engine.Parts;
            Parts = new ObservableCollection<string>(parts);
        }



        #endregion


        #region Document Pane - Navigation


        private DesignerNavigatorDocument NewPageTab(string href, string background, Action<DesignerNavigator> onLoaded)
        {
            // A page already open is shown in its tab rather than opened again
            var open = FindTab(DesignerNavigatorDocument.PageKind, href);
            if (open != null)
                return ShowTab(open);

            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator) { Kind = DesignerNavigatorDocument.PageKind };
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            navigator.ModifiedChanged += NavigatorModifiedChangedHandler;
            navigator.ElementsChanged += NavigatorElementsChangedHandler;
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
            // A layout already open is shown in its tab rather than opened again
            var open = FindTab(DesignerNavigatorDocument.LayoutKind, href);
            if (open != null)
                return ShowTab(open);

            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator) { Kind = DesignerNavigatorDocument.LayoutKind };
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            navigator.ModifiedChanged += NavigatorModifiedChangedHandler;
            navigator.ElementsChanged += NavigatorElementsChangedHandler;
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
            // A part already open is shown in its tab rather than opened again
            var open = FindTab(DesignerNavigatorDocument.PartKind, href);
            if (open != null)
                return ShowTab(open);

            var navigator = new DesignerNavigator(Engine, background);

            var doc = new DesignerNavigatorDocument(navigator) { Kind = DesignerNavigatorDocument.PartKind };
            DocumentPane.Children.Add(doc);
            navigator.NavigatorDocument = doc;
            navigator.Navigating += NavigatorNavigatingHandler;
            navigator.PageNameChanged += doc.TitleChangedHandler;
            navigator.SelectionChanged += NavigatorSelectionChangedHandler;
            navigator.ModifiedChanged += NavigatorModifiedChangedHandler;
            navigator.ElementsChanged += NavigatorElementsChangedHandler;
            //navigator.NavigationFailed += NavigatorNavigationFailedHandler;
            navigator.PageName = href;
            navigator.SelectedDrawingTool = DefaultDrawingTool;
            DocumentPane.SelectedContentIndex = DocumentPane.Children.Count - 1;
            CurrentDesigner = navigator;

            NavigateToPart(CurrentDesigner, href);

            navigator.Focus();
            return doc;
        }



        // A link followed in Navigate mode; NavigateToPage shows a page open in another tab there
        private void NavigatorNavigatingHandler(DesignerNavigator nav, string href)
        {
            NavigateToPage(nav, href);
        }


        /// <summary>The tab that shows the document of a kind and name, if one does; names compare ignoring case.</summary>
        private DesignerNavigatorDocument FindTab(string kind, string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return null;
            return DocumentPane.Children.OfType<DesignerNavigatorDocument>()
                .FirstOrDefault(d => d.Kind == kind && string.Equals(d.Navigator.PageName, href, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>Makes an open tab the current one (DocumentTabChangeHandler then selects its page and outline).</summary>
        private DesignerNavigatorDocument ShowTab(DesignerNavigatorDocument doc)
        {
            DocumentPane.SelectedContentIndex = DocumentPane.Children.IndexOf(doc);
            return doc;
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
            // A page open in another tab is shown there rather than opened in this one as well
            var open = FindTab(DesignerNavigatorDocument.PageKind, href);
            if (open != null && open.Navigator != designer)
            {
                ShowTab(open);
                return;
            }
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
            if (LayerOutline != null)
                foreach (var proxy in LayerOutline)
                    proxy.Detach();
            LayerOutline = null;
        }


        // Brings the Outline up to date with the current designer's page in place: the entries of elements that stay are kept as they are (expanded, selected); only those of elements added, removed or moved change
        public void PopulateOutlineTree()
        {
            if (CurrentDesigner == null)
            {
                ClearOutlineTree();
                return;
            }
            if (LayerOutline == null)
                LayerOutline = new ObservableCollection<UIElementProxy>();
            OutlineSync(CurrentDesigner.Designer.Canvas, LayerOutline);
            UpdateOutlineHighlight();
        }


        // The elements a panel's entry lists: the page's layouts, a layout's layers and a layer's or group's elements, topmost first; a stack panel's elements in their order in it
        private static List<FrameworkElement> OutlineChildren(Panel panel)
        {
            var children = panel is Layout ? panel.Children.OfType<Layer>().Cast<FrameworkElement>().ToList() : panel.Children.OfType<FrameworkElement>().ToList();
            if (!(panel is Stack))
                children.Reverse();
            return children;
        }


        // A new entry for an element of a panel
        private static UIElementProxy OutlineProxy(Panel panel, FrameworkElement element)
        {
            if (panel is Layout)
                return new UILayerProxy(element);
            if (!(panel is Layer || panel is Group || panel is Stack))
                return element is Layout ? new UILayoutProxy(element) : new UIElementProxy(element);
            switch (element)
            {
                case Part _: return new UIPartProxy(element);
                case Group _: return new UIGroupProxy(element);
                case Stack _: return new UIStackProxy(element);
                case Box _: return new UIBoxProxy(element);
                case ImageBox _: return new UIImageProxy(element);
                case WireFlowText _: return new UITextProxy(element);
                default: return new UIElementProxy(element);
            }
        }


        // Makes a panel's entries those of its elements, keeping the entries of the elements they already show; true when any entry was added, removed or moved
        private static bool OutlineSync(Panel panel, ObservableCollection<UIElementProxy> proxies)
        {
            bool changed = false;
            var children = OutlineChildren(panel);
            var wanted = new HashSet<FrameworkElement>(children);
            for (int i = proxies.Count - 1; i >= 0; i--)
                if (!wanted.Contains(proxies[i].UIElement))
                {
                    proxies[i].Detach();
                    proxies.RemoveAt(i);
                    changed = true;
                }
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                int at = -1;
                for (int j = i; j < proxies.Count && at < 0; j++)
                    if (proxies[j].UIElement == child)
                        at = j;
                UIElementProxy proxy;
                if (at < 0)
                {
                    proxy = OutlineProxy(panel, child);
                    proxies.Insert(i, proxy);
                    changed = true;
                }
                else
                {
                    proxy = proxies[at];
                    if (at != i)
                    {
                        proxies.Move(at, i);
                        changed = true;
                    }
                }
                // A count shown in the entry follows its elements
                if (proxy is UICanvasProxy canvas && (child is Layer || child is Group || child is Stack) && OutlineSync((Panel)child, canvas.Elements))
                    canvas.Refresh();
            }
            return changed;
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
        public ICommand UndoCmd { get; set; }
        /// <summary>Adds a layer above the current one (the Outline's caption and menu).</summary>
        public ICommand AddLayerCmd { get; set; }
        /// <summary>Deletes the elements selected, or else the layer chosen in the Outline (the Outline's caption and menu).</summary>
        public ICommand OutlineDeleteCmd { get; set; }
        public ICommand RedoCmd { get; set; }
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
        public ICommand MovePageUpCmd { get; set; }
        public ICommand MovePageDownCmd { get; set; }
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
            AddLayerCmd = new RelayCommand(o => CurrentDesigner?.AddLayer(), o => CurrentDesigner?.CurrentPage is FlowPage);
            OutlineDeleteCmd = new RelayCommand(o => OutlineDelete(), o => CanOutlineDelete());
            UndoCmd = new RelayCommand(o => CurrentDesigner?.Undo(), o => CurrentDesigner?.CanUndo ?? false);
            RedoCmd = new RelayCommand(o => CurrentDesigner?.Redo(), o => CurrentDesigner?.CanRedo ?? false);
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
            MovePageUpCmd = new RelayCommand(o => MovePage((string)ListBoxPages.SelectedItem, SelectedPageIndex - 1), o => SelectedPageIndex > 0);
            MovePageDownCmd = new RelayCommand(o => MovePage((string)ListBoxPages.SelectedItem, SelectedPageIndex + 1),
                o => SelectedPageIndex >= 0 && SelectedPageIndex < Pages.Count - 1);
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
