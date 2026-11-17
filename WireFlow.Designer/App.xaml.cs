using System.Windows;

namespace WireFlow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public IFlowRepository Repository { get; set; }
        public WireFlowEngine Engine { get; set; }


        // The splash screen, at once, centered on the window about to be shown (DesignerSplash)
        public App()
        {
            DesignerSplash.ShowAtStart();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Brushes and colors in the property grid open the color dialog
            ColorPropertyEditor.Register();
            // The TextAlignment of texts and boxes is chosen as a position in the frame, as in PDFTools
            TextPositionPropertyEditor.Register();
            // The font of texts and boxes is chosen in the font dialog of Optimax's programs
            FontPropertyEditor.Register();
            LayoutPropertyEditor.Register();

            // Opening a folder project (.project) converts it into a .wf file beside it
            Repository = new SqliteRepository() { ConvertFolderProjects = true };
            Engine = new WireFlowEngine(Repository);
            MainDesignerWindow wnd = new MainDesignerWindow(Engine);


            wnd.Show();
        }

    }
}
