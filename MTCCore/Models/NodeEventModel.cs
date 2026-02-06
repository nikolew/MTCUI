

namespace MTCCore.Models
{
    public class NodeEventModel
    {
        public int Id { get; set; }
        public bool Online { get; set; }
        public int MissedFrames { get; set; }
        public int LastSeenMs { get; set; }
    }
}
