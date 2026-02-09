using Microsoft.EntityFrameworkCore;
using MTCCore.DataBase;
using MTCCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Repositories
{
    public interface INodeRepository
    {
        void AddNode(NodeEntity node);
        Task<NodeEntity> GetByIdAsync(int id);
        Task<List<NodeEntity>> GetAll();
        Task<NodeEntity> GetNodeByUniqueId(string uniqueId);
        Task Update(NodeEntity nodeEntity);
    }

    public class NodeRepository : INodeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public NodeRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async void AddNode(NodeEntity node)
        {
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();
        }

        public Task<NodeEntity> GetByIdAsync(int id)
        {
            return _dbContext.Nodes.SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<List<NodeEntity>> GetAll()
        {
            return _dbContext.Nodes.Include(a => a.Position).Include(g => g.GroupEnttity).ToListAsync();
        }

        public Task<NodeEntity> GetNodeByUniqueId(string uniqueId)
        {
            return _dbContext.Nodes.Include(a => a.Position).SingleOrDefaultAsync(x => x.NodeUniqueId == uniqueId);
        }

        public async Task Update(NodeEntity nodeEntity)
        {
             _dbContext.Nodes.Update(nodeEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
