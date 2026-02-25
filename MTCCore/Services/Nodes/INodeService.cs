using MTCCore.DTO.Nodes;
using MTCCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Nodes
{
    public interface INodeService
    {
        Task AddNodeAsync(int groupId, NodeModel node);

        bool NodeExists(string uniqueId);
        NodeModel GetNodeByUniqueId(string uniqueId);
        List<NodeModel> GetAllNodes();
        Task UpdateNodes(IEnumerable<NodeModel> nodes);
        Task UpdateNode(NodeModel node);




        Task<List<ReadNodeDto>> GetAllAsync();

        Task CreateNodeAsync(CreateNodeDto dto);
        Task UpdateNodeAsync(SaveNodeDto dto);
        Task UpdateNodesAsync(List<SaveNodeDto> dto);
    }
}