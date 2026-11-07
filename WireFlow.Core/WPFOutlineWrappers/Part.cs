namespace WireFlow
{
    public class Part : BaseElement, ILinkable
    {
        public string TemplateName { get; set; }

        [System.ComponentModel.Category("Navigation")]
        public string Href { get; set; }
    }

}
