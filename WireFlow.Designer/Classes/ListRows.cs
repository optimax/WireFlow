using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MaxServices.Core.Themes;

namespace WireFlow
{
    /// <summary>
    /// Close rows for lists. The Fluent themes pad a list's items by 12 px, which puts their rows 42 to 46 px apart; every
    /// list the Designer shows has its rows close together instead (the user's requirement). The padding is set on each row
    /// as a local value, which keeps the theme's item template: an ItemContainerStyle would replace the theme's style, and
    /// one based on it would keep the style of the theme current when it was made. Classic pads its rows by 4,1 px, close
    /// already, so it keeps its own.
    /// </summary>
    public static class ListRows
    {
        /// <summary>Rows with next to no room between their lines, as a font dialog's lists have them.</summary>
        public static readonly Thickness Tight = new Thickness(6, 1, 6, 1);

        /// <summary>Rows as tall as a Fluent menu item (27 px), as the lists of the main window's panes have them.</summary>
        public static readonly Thickness Compact = new Thickness(8, 3, 8, 3);


        /// <summary>Gives a list's rows this padding, now and as it makes them: for new items, and as it scrolls.</summary>
        public static void Keep(ListBox list, Thickness padding)
        {
            list.ItemContainerGenerator.StatusChanged += (sender, e) => Apply(list, padding);
            Apply(list, padding);
        }


        /// <summary>Gives the rows a list has made this padding, or, in the Classic theme, their own again.</summary>
        public static void Apply(ListBox list, Thickness padding)
        {
            if (list.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;
            bool classic = AppTheme.Choice == ThemeChoice.Classic;
            UIElement host = null;
            foreach (var item in list.Items)
                if (list.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem container)
                {
                    if (classic ? container.ReadLocalValue(Control.PaddingProperty) == DependencyProperty.UnsetValue : container.Padding == padding)
                        continue;
                    if (classic)
                        container.ClearValue(Control.PaddingProperty);
                    else
                        container.Padding = padding;
                    host = host ?? VisualTreeHelper.GetParent(container) as UIElement;
                }
            // The list makes its rows while it is laid out, and a row changed then can keep the height it had (the rows above
            // the one a list was scrolled to did): the panel holding the rows is laid out again, once this pass is done
            if (host != null)
                list.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new System.Action(() =>
                {
                    host.InvalidateMeasure();
                    foreach (var item in list.Items)
                        (list.ItemContainerGenerator.ContainerFromItem(item) as UIElement)?.InvalidateMeasure();
                }));
        }
    }
}
