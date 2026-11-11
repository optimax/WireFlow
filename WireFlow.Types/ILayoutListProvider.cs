using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireFlow
{
    /// <summary>What the New Page and New Layout dialogs offer as the layout of a new page or layout.</summary>
    public interface ILayoutListProvider
    {
        /// <summary>The names of the layouts.</summary>
        IList<string> LayoutNames { get; }
    }
}
