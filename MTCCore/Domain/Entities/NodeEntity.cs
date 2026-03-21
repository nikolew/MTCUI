using MTCCore.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTCCore.Domain.Entities
{
    public class NodeEntity
    {
        public int Id { get; set; }
        public int NodeIdentity { get; set; }
        public string NodeUniqueId { get; set; }
        public string Distance { get; set; }
        //public Group TargetGroup { get; set; }
        public TargetType TargetType { get; set; }

        public PositionEntity Position { get; set; }

        [Column("GroupId")]
        public int GroupEnttityId { get; set; }
        public GroupEntity GroupEnttity { get; set; }
    }
}
