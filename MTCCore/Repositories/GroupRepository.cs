using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTCCore.Repositories
{
    public interface IGroupRepository
    {
        List<GroupEntity> GetAll();
        List<string> GetTimes(string groupName);
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
            return [.. _dbContext.Groups.Include(t => t.Times)];
        }

        public List<string>GetTimes(string groupName)
        {
            var times = _dbContext.Groups.Where(g => g.GroupName == groupName).SelectMany(m => m.Times).Select(t => t.Time).ToList();

            return times;
        }
    }
}
