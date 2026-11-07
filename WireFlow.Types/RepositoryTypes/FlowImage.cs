namespace WireFlow
{
    public class FlowImage: FlowElementBase
    {
        private string image;
        private FlowStretch stretch;

        public string Image { get => image; set => Set(ref image, value); }
        public FlowStretch Stretch { get => stretch; set => Set(ref stretch, value); }

        public FlowImage()
        {
            //Stretch = Stretch.Fill;
        }
    }
}
