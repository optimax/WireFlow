using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WireFlow
{
    public class BaseDesignerObject: UserControl, ILinkable
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

        [Category("Navigation")]
        public string Href { get; set; }


        /// <summary>
        /// The content is what the element's constructor builds, and its properties forward to it, so a copy made through
        /// XAML (Duplicate) builds its own rather than taking a second one.
        /// </summary>
        public override bool ShouldSerializeContent() => false;

    }
}
