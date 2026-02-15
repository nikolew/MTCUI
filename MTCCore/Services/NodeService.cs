using MTCCore.Entities;
using MTCCore.Models;
using MTCCore.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MTCCore.Services
{
    public interface INodeService
    {
        void AddNode(NodeModel node);
        bool NodeExists(string uniqueId);
        NodeModel GetNodeByUniqueId(string uniqueId);
        List<NodeModel> GetAllNodes();
        void UpdateNodes(IEnumerable<NodeModel> nodes);
        Task UpdateNode(NodeModel node);
    }

    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IGroupRepository _groupRepository;

        public NodeService( INodeRepository nodeRepository, IGroupRepository groupRepository)
        {
            _nodeRepository = nodeRepository;
            _groupRepository = groupRepository;
        }

        public bool NodeExists(string uniqueId)
        {
            var node = _nodeRepository.GetNodeByUniqueId(uniqueId).Result;
            return node != null;
        }

        public void AddNode(NodeModel node)
        {        
            var positionEntity = new PositionEntity
            {
                X = (int)node.Position.X,
                Y = (int)node.Position.Y
            };

            var gr = _groupRepository.GetAll().Where(g => g.Id == 1).FirstOrDefault();

            var nodeEntity = new NodeEntity
            {
                NodeUniqueId = node.UniqueId,
                NodeIdentity = int.Parse(node.TargetId),
                Distance = "0",
                Position = positionEntity,
                TargetType = Enums.TargetType.Default,
                GroupEnttity = gr
            };

            _nodeRepository.AddNode(nodeEntity);
        }

        public NodeModel GetNodeByUniqueId(string uniqueId)
        { 
            var node = _nodeRepository.GetNodeByUniqueId(uniqueId).Result;

            if(node == null)
                return null;

            var newNode = new NodeModel
            {
                UniqueId = node.NodeUniqueId,
                TargetId = node.NodeIdentity.ToString(),
                Position = new Point(node.Position.X, node.Position.Y),
                TargetType = node.TargetType,
                State = Enums.TargetState.TargetRaised,
                Distance = node.Distance
            };

            return newNode; 
        }

        public List<NodeModel> GetAllNodes()
        {
            var nodes = _nodeRepository.GetAll().Result;

            return nodes.Select(node => new NodeModel
                {
                    UniqueId = node.NodeUniqueId,
                    TargetId = node.NodeIdentity.ToString(),
                    Position = new Point(node.Position.X, node.Position.Y),
                    TargetType = node.TargetType,
                    State = Enums.TargetState.TargetOffline,
                    Distance = node.Distance,
                    Group = node.TargetGroup
                }).ToList();
        }

        public void UpdateNodes(IEnumerable<NodeModel> nodes)
        {
            foreach (var node in nodes)
            {
                var nodeEntity = _nodeRepository.GetNodeByUniqueId(node.UniqueId).Result;
                if (nodeEntity == null) 
                    continue;
                
                nodeEntity.Position.X = (int)node.Position.X;
                nodeEntity.Position.Y = (int)node.Position.Y;
                nodeEntity.TargetType = node.TargetType;
                nodeEntity.Distance = node.Distance;
                nodeEntity.TargetGroup = node.Group;

                _nodeRepository.Update(nodeEntity);
            }

        }

        public async Task UpdateNode(NodeModel node)
        {
            var nodeEntity = _nodeRepository.GetNodeByUniqueId(node.UniqueId).Result;
            if (nodeEntity == null) 
                return;
            
            nodeEntity.Position.X = (int)node.Position.X;
            nodeEntity.Position.Y = (int)node.Position.Y;
            nodeEntity.TargetType = node.TargetType;
            nodeEntity.Distance = node.Distance;
            nodeEntity.TargetGroup = node.Group;
            await _nodeRepository.Update(nodeEntity);
            
            
        }
    }
}
