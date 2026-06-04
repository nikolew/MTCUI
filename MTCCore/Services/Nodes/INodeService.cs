using MTCCore.DTO.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Nodes
{
    public interface INodeService
    {
        
        Task<List<ReadNodeDto>> GetAllAsync();

        Task CreateNodeAsync(CreateNodeDto dto);
        Task UpdateNodeAsync(SaveNodeDto dto);
        Task UpdateNodesAsync(List<SaveNodeDto> dto);
        Task<ReadNodeDto> GetNodeByUniqueIdAsync(int uniqueId);



        //====================================================
        void LoadScene();
        Task SaveScene(List<SaveNodeDto> nodesSave);
        void NodeCommand(Envelope packet);
    }
}