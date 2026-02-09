using MTCCore.DataBase;
using MTCCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTCCore.Repositories
{
    public interface IGroupRepository
    {
        List<GroupEntity> GetAll();
    }

    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GroupRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<GroupEntity> GetAll()
        {
            return _dbContext.Groups.ToList();
        }
    }
}
