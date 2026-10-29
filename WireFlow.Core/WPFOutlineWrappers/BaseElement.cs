using System.Windows.Controls;

namespace WireFlow
{

    /// <summary>
    /// Base class for all composite WireFlow elements, such as Groups, Layers, etc.
    /// </summary>
    public class BaseElement : Canvas
    {
        public double Left
        {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }
    }

}
