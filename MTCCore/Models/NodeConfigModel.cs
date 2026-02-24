using MTCCore.Domain.Enums;

namespace MTCCore.Models
{
    public class NodeConfigModel
    {
        public int NodeId { get; set; }
        public int GroupId { get; set; }
        public LightMode Light { get; set; }
    }
}
