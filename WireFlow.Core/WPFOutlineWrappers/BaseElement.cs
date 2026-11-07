using System.ComponentModel;
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

        /// <summary>
        /// Not listed in Properties: it gives the panel's children to an ItemsControl that a page element is not in, after
        /// which WPF refuses to give them to anything else, and the Designer, which reads them on a click, crashed.
        /// </summary>
        [Browsable(false)]
        public new bool IsItemsHost
        {
            get => base.IsItemsHost;
            set => base.IsItemsHost = value;
        }
    }

}
