using System.IO;
using System.Xml.Serialization;
using OptimaxTools.Dock.Core;
using OptimaxTools.Dock.Core.Serialization.Dto;

namespace WireFlow
{
    /// <summary>
    /// Writes and reads the docking layout of the Designer's panes (where each is docked, floating or hidden, and their
    /// sizes) as XML, which the layout's DTOs are made for.
    /// </summary>
    public class DockLayoutSerializer : LayoutSerializerBase
    {
        private static readonly XmlSerializer Xml = new XmlSerializer(typeof(LayoutRootDto));


        public DockLayoutSerializer(IDockingManager manager) : base(manager)
        {
        }


        protected override void SerializeCore(Stream stream, LayoutRootDto dto) => Xml.Serialize(stream, dto);


        protected override LayoutRootDto DeserializeCore(Stream stream) => (LayoutRootDto)Xml.Deserialize(stream);
    }
}
