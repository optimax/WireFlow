namespace WireFlow
{
    public class FlowProject: FlowBase
    {
        /// <summary>
        /// The page background of a new project: a gray that differs from the area around the page in every theme (#333333 or #CCCCCC)
        /// without glaring in the dark one.
        /// </summary>
        public const string DEFAULT_BACKGROUND = "#555";

        private long revisionNo;
        private double width;
        private double height;
        private string background;
        private string home;

        public long RevisionNo { get => revisionNo; set => Set(ref revisionNo, value); }
        public double Width { get => width; set => Set(ref width, value); }
        public double Height { get => height; set => Set(ref height, value); }
        public string Background { get => background; set => Set(ref background, value); }
        public string Home { get => home; set => Set(ref home, value); }

        public FlowProject()
        {
            Width = 1024;
            Height = 768;
            Background = DEFAULT_BACKGROUND;
            Home = "index";
        }
    }
}
