using System.Windows.Media;

namespace WireFlow
{
    public class FlowImage: FlowElementBase
    {
        public string Image { get; set; }
        public Stretch Stretch { get; set; }

        public FlowImage()
        {
            //Stretch = Stretch.Fill;
        }
    }
}
