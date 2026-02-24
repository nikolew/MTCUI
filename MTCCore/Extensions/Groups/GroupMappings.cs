using MTCCore.Domain.Entities;
using MTCCore.DTO.Grups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTCCore.Extensions.Groups
{
    public static class GroupMappings
    {
        public static GroupReadDto ToReadDto(this GroupEntity e)
        {
            return new GroupReadDto(
                e.Id,
                e.GroupName,
                e.Times
                 .OrderBy(t => t.Time)
                 .Select(t => t.Time)
                 .ToList()
            );
        }
    }
}
