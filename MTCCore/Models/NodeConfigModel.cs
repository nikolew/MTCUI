using MTCCore.Domain.Enums;
using Group = MTCCore.Domain.Enums.Group;

namespace MTCCore.Models
{
    public class NodeConfigModel
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public LightMode Light { get; set; }
    }
}
