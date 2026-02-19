using System.ComponentModel.DataAnnotations.Schema;

namespace MTCCore.Domain.Entities
{
    [Table("Time")]
    public class TimeEntity
    {
        public int Id { get; set; }
        public string Time { get; set; }

        public int GroupEntityId { get; set; }
        public GroupEntity GroupEntity { get; set; }

    }
}
