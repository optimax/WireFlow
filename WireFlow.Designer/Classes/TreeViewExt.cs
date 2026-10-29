using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WireFlow
{
    public static class TreeViewExt
    {


        public static void ExpandAll(this TreeView treeView)
        {
            Expand(treeView);
        }


        private static void Expand(ItemsControl items, bool expand = true)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    Expand(childControl, expand);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if (item != null)
                    item.IsExpanded = true;
            }
        }


        //private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        //{

        //    foreach (object item in this.myTV.Items)
        //    {
        //        TreeViewItem treeItem = this.myTV.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
        //        if (treeItem != null)
        //            ExpandAll(treeItem, true);
        //        treeItem.IsExpanded = true;
        //    }
        //}

    }
}
