namespace WireFlow
{
    /// <summary>
    /// A stack panel (kind "stack"): its children one after another in its Direction, each keeping its own size in that
    /// direction and stretched across the panel in the other one, as WPF's StackPanel lays them out.
    /// </summary>
    public class FlowStack : FlowGroup
    {
        private FlowOrientation direction = FlowOrientation.Vertical;

        public FlowOrientation Direction { get => direction; set => Set(ref direction, value); }
    }
}
