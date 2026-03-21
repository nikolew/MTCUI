using MTCCore.Domain.Enums;
using Windows.Foundation;



namespace MTCCore.DTO.Nodes
{
    public class SaveNodeDto
    {
        public string UniqueNodeId { get; set; }
        public int NodeId { get; set; }
        public string Distance { get; set; }
        public Point Position { get; set; }
        public TargetType TargetType { get; set; }
        public string GroupName { get; set; }
        public int GroupId { get; set; }
    }
}
