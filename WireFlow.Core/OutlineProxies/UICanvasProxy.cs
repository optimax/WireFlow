using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WireFlow
{
    public abstract class UICanvasProxy: UIElementProxy
    {
        public ObservableCollection<UIElementProxy> Elements { get; set; } = new ObservableCollection<UIElementProxy>();

        public UICanvasProxy(FrameworkElement element): base(element)
        {
        }


        protected override void RefreshElements()
        {
            foreach (var proxy in Elements)
            {
                proxy.Refresh();
                if (proxy is UICanvasProxy canvas)
                    canvas.RefreshElements();
            }
        }


        public override void Detach()
        {
            base.Detach();
            foreach (var proxy in Elements)
                proxy.Detach();
        }
    }
}
