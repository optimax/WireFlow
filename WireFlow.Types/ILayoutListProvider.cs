using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireFlow
{
    public interface ILayoutListProvider
    {
        IList<string> Layouts { get; }
    }
}
