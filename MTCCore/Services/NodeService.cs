using MTCCore.Entities;
using MTCCore.Models;
using MTCCore.Repositories;
using System.Collections.Generic;
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
    }

    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;

        public NodeService( INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public bool NodeExists(string uniqueId)
        {
            var node = _nodeRepository.GetNodeByUniqueId(uniqueId).Result;
            if (node != null)
                return true;

            return false;
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
                TargetType = Enums.TargetType.Default

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

            var nodeModels = new List<NodeModel>();

            foreach (var node in nodes)
            {
                var newNode = new NodeModel
                {
                    UniqueId = node.NodeUniqueId,
                    TargetId = node.NodeIdentity.ToString(),
                    Position = new Point(node.Position.X, node.Position.Y),
                    TargetType = node.TargetType,
                    State = Enums.TargetState.TargetRaised,
                    Distance = node.Distance
                };
                nodeModels.Add(newNode);
            }
            return nodeModels;
        }

        public void UpdateNodes(IEnumerable<NodeModel> nodes)
        {
            foreach (var node in nodes)
            {
                var nodeEntity = _nodeRepository.GetNodeByUniqueId(node.UniqueId).Result;
                if (nodeEntity != null)
                {
                    nodeEntity.Position.X = (int)node.Position.X;
                    nodeEntity.Position.Y = (int)node.Position.Y;
                    nodeEntity.TargetType = node.TargetType;
                    nodeEntity.Distance = node.Distance;

                    _nodeRepository.Update(nodeEntity);
                }
            }

        }
    }
}
