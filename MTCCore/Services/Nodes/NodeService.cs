using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using MTCCore.Domain.Enums;
using MTCCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MTCCore.Services.Nodes
{
    public class NodeService : INodeService
    {
        private ApplicationDbContext _dbContext;

        public NodeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool NodeExists(string uniqueId)
        {
            var node = _dbContext.Nodes
                .Include(a => a.Position)
                .SingleOrDefaultAsync(x => x.NodeUniqueId == uniqueId);
            return node != null;
        }

        public void AddNode(NodeModel node)
        {        
            var positionEntity = new PositionEntity
            {
                X = (int)node.Position.X,
                Y = (int)node.Position.Y
            };

            var nodeEntity = new NodeEntity
            {
                NodeUniqueId = node.UniqueId,
                NodeIdentity = int.Parse(node.TargetId),
                Distance = "0",
                Position = positionEntity,
                TargetType = TargetType.Default
            };

            _dbContext.Nodes.Add(nodeEntity);
        }

        public async Task AddNodeAsync(int groupId, NodeModel node)
        {
            var groupExists =  _dbContext.Groups.Where(g => g.Id == groupId).SingleOrDefault();
            
            if (groupExists is null)
                throw new InvalidOperationException("Group not found.");

            var positionEntity = new PositionEntity
            {
                X = (int)node.Position.X,
                Y = (int)node.Position.Y
            };

            var newnode = new NodeEntity
            {
                GroupEnttityId = groupId,
                GroupEnttity = groupExists,
                NodeUniqueId = node.UniqueId,
                NodeIdentity = int.Parse(node.TargetId),
                Distance = "0",
                Position = positionEntity,
                TargetType = TargetType.Default
            };

            _dbContext.Nodes.Add(newnode);
            await _dbContext.SaveChangesAsync();
            return ;
        }

        public NodeModel GetNodeByUniqueId(string uniqueId)
        { 
            var node = _dbContext.Nodes
                .Include(a => a.Position)
                .SingleOrDefaultAsync(x => x.NodeUniqueId == uniqueId).Result;

            if (node == null)
                return null;

            var newNode = new NodeModel
            {
                UniqueId = node.NodeUniqueId,
                TargetId = node.NodeIdentity.ToString(),
                Position = new Point(node.Position.X, node.Position.Y),
                TargetType = node.TargetType,
                State = TargetState.TargetRaised,
                Distance = node.Distance
            };

            return newNode; 
        }

        public List<NodeModel> GetAllNodes()
        {
            var nodes = _dbContext.Nodes;

            return nodes.Select(node => new NodeModel
                {
                    UniqueId = node.NodeUniqueId,
                    TargetId = node.NodeIdentity.ToString(),
                    Position = new Point(node.Position.X, node.Position.Y),
                    TargetType = node.TargetType,
                    State = TargetState.TargetOffline,
                    Distance = node.Distance,
                    Group = node.TargetGroup
                }).ToList();
        }

        public async Task UpdateNodes(IEnumerable<NodeModel> nodes)
        {
            foreach (var node in nodes)
            {
                var nodeEntity = _dbContext.Nodes
                    .Include(a => a.Position)
                    .SingleOrDefaultAsync(x => x.NodeUniqueId == node.UniqueId).Result;

                if (nodeEntity == null) 
                    continue;
                
                nodeEntity.Position.X = (int)node.Position.X;
                nodeEntity.Position.Y = (int)node.Position.Y;
                nodeEntity.TargetType = node.TargetType;
                nodeEntity.Distance = node.Distance;
                //nodeEntity.TargetGroup = node.Group;

                _dbContext.Update(nodeEntity);
                await _dbContext.SaveChangesAsync();
            }

        }

        public async Task UpdateNode(NodeModel node)
        {
            var nodeEntity = _dbContext.Nodes
                .Include(a => a.Position)
                .SingleOrDefaultAsync(x => x.NodeUniqueId == node.UniqueId).Result;

            if (nodeEntity == null) 
                return;
            
            nodeEntity.Position.X = (int)node.Position.X;
            nodeEntity.Position.Y = (int)node.Position.Y;
            nodeEntity.TargetType = node.TargetType;
            nodeEntity.Distance = node.Distance;
            nodeEntity.TargetGroup = node.Group;

            _dbContext.Update(nodeEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
