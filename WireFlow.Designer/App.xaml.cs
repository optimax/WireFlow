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

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            // Opening a folder project (.project) converts it into a .wf file beside it
            Repository = new SqliteRepository() { ConvertFolderProjects = true };
            Engine = new WireFlowEngine(Repository);
            MainDesignerWindow wnd = new MainDesignerWindow(Engine);


            wnd.Show();
        }

    }
}
