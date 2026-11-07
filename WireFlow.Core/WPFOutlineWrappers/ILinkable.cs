namespace WireFlow
{
    /// <summary>
    /// An element that can be a hyperlink: its Href names the page a click on it goes to. A click on an element inside a
    /// group, stack panel or part follows the innermost link around it, its own first.
    /// </summary>
    public interface ILinkable
    {
        string Href { get; set; }
    }
}
