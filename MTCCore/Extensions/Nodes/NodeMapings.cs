using MTCCore.Domain.Entities;
using MTCCore.DTO.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Extensions.Nodes
{
    public static class NodeMapings
    {
        public static ReadNodeDto ToReadDto(this NodeEntity e)
        {
            return new ReadNodeDto
            {
                UniqueNodeId = e.NodeUniqueId,
                NodeId = e.Id,
                Distance = e.Distance,
                GroupId = e.GroupEnttityId,
                TargetType = e.TargetType,
            };
        }
    }
}
