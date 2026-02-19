using MTCCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Nodes
{
    public interface INodeService
    {
        void AddNode(NodeModel node);
        Task AddNodeAsync(int groupId, NodeModel node);

        bool NodeExists(string uniqueId);
        NodeModel GetNodeByUniqueId(string uniqueId);
        List<NodeModel> GetAllNodes();
        Task UpdateNodes(IEnumerable<NodeModel> nodes);
        Task UpdateNode(NodeModel node);
    }
}