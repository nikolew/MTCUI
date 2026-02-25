using MTCCore.Domain.Enums;
using Windows.Foundation;

namespace MTCCore.DTO.Nodes
{
    public class CreateNodeDto
    {
        public string UniqueNodeId { get; set; }
        public int NodeId { get; set; }
        public string Distance { get; set; }
        public Point Position { get; set; }
        public int GroupId { get; set; }
        public int Rssi { get; set; }
        public int Snr { get; set; }
        public float BattVoltage { get; set; }
        public TargetType TargetType { get; set; }
        public TargetState State { get; set; }
    }
}
