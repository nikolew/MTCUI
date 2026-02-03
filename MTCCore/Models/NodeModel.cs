using MTCCore.Enums;
using Windows.Foundation;

namespace MTCCore.Models
{
    public class NodeModel
    {
        public string UniqueId { get; set; }
        public string TargetId { get; set; }
        public string Distance { get; set; }
        public Point Position { get; set; }
        public TargetType TargetType { get; set; }
        public TargetState State { get; set; }
    }
}
