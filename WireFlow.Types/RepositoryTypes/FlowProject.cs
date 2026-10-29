namespace WireFlow
{
    public class FlowProject: FlowBase
    {
        public long RevisionNo { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Background { get; set; }
        public string Home { get; set; }

        public FlowProject()
        {
            Width = 1024;
            Height = 768;
            Background = "#333";
            Home = "index";
        }

    }

}
