namespace WireFlow
{
    /// <summary>
    /// A panel (kind "panel"): a container that places its children by their Alignment, as a Delphi panel does. A child
    /// aligned Top, Bottom, Left or Right lies along that side of the room the panel has left, across all of it, keeping
    /// its own size the other way; one aligned Fill takes the room left; one with no alignment stays where its own
    /// position puts it in the panel.
    /// </summary>
    public class FlowPanel : FlowGroup
    {
    }
}
