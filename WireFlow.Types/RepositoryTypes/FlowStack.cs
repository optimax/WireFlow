namespace WireFlow
{
    /// <summary>
    /// A stack panel (kind "stack"): its children one after another in its Direction, each keeping its own size in that
    /// direction and stretched across the panel in the other one, as WPF's StackPanel lays them out; from its top or left
    /// (StackingFrom Beginning), or from its bottom or right (Ending), the first child at that end and the next ones
    /// before it.
    /// </summary>
    public class FlowStack : FlowGroup
    {
        private FlowOrientation direction = FlowOrientation.Vertical;
        private FlowStackingFrom stackingFrom = FlowStackingFrom.Beginning;

        public FlowOrientation Direction { get => direction; set => Set(ref direction, value); }

        /// <summary>The end the children are stacked from; a file written before it has none, which is Beginning.</summary>
        public FlowStackingFrom StackingFrom { get => stackingFrom; set => Set(ref stackingFrom, value); }
    }
}
