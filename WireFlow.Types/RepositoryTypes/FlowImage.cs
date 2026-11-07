namespace WireFlow
{
    public class FlowImage: FlowElementBase
    {
        public string Image { get; set; }
        public FlowStretch Stretch { get; set; }

        public FlowImage()
        {
            //Stretch = Stretch.Fill;
        }
    }
}
