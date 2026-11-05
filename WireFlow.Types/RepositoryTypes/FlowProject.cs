namespace WireFlow
{
    public class FlowProject: FlowBase
    {
        /// <summary>
        /// The page background of a new project: a gray that differs from the area around the page in every theme (#333333 or #CCCCCC)
        /// without glaring in the dark one.
        /// </summary>
        public const string DEFAULT_BACKGROUND = "#555";

        public long RevisionNo { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Background { get; set; }
        public string Home { get; set; }

        public FlowProject()
        {
            Width = 1024;
            Height = 768;
            Background = DEFAULT_BACKGROUND;
            Home = "index";
        }

    }

}
