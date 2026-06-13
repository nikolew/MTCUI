using MTCCore.Domain.Entities;
using MTCCore.DTO.Grups;
using System.Linq;
using Windows.UI;

namespace MTCCore.Extensions.Groups
{
    public static class GroupMappings
    {
        public static GroupReadDto ToReadDto(this GroupEntity e)
        {
            var color = Color.FromArgb(e.GroupColor.A, e.GroupColor.R, e.GroupColor.G, e.GroupColor.B);
            return new GroupReadDto(
                e.Id,
                e.GroupName,
                e.Times
                 .OrderBy(t => t.Time)
                 .Select(t => t.Time)
                 .ToList(),
                color
            );
        }
    }
}
