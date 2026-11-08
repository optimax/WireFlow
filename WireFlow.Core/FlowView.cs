using System.Windows;

namespace WireFlow
{
    /// <summary>
    /// Ties the WPF elements that WireFlowEngine draws (layers, elements) to the DOM nodes they show: each one's Node is
    /// its node. The engine keeps them in step with their nodes; other code finds an element's node here.
    /// </summary>
    public static class FlowView
    {
        public static readonly DependencyProperty NodeProperty = DependencyProperty.RegisterAttached(
            "Node", typeof(FlowBase), typeof(FlowView), new PropertyMetadata(null));


        /// <summary>The DOM node an element shows; null for one that shows none (a grid, the rubber band).</summary>
        public static FlowBase GetNode(DependencyObject element) => element?.GetValue(NodeProperty) as FlowBase;


        public static void SetNode(DependencyObject element, FlowBase node) => element.SetValue(NodeProperty, node);
    }
}
