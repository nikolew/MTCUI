using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTCCore.Services.Scheduling
{
    public class SchedulingService : ISchedulingService
    {
        private readonly ApplicationDbContext _dbContext;

        public SchedulingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddTimeToGroupAsync(string groupName, string time)
        {
            var group = _dbContext.Groups.Where(g => g.GroupName == groupName).SingleOrDefault();

            if (group is null) 
                throw new InvalidOperationException("Group not found.");

            var entry = new TimeEntity
            {
                GroupEntityId = group.Id,
                Time = time
            };

            _dbContext.Times.Add(entry);
            await _dbContext.SaveChangesAsync();

            return entry.Id;
        }

        public async Task<List<string>> GetTimesForGroupAsync(string groupName)
        {
            var times = await _dbContext.Times
                .Where(t => t.GroupEntity.GroupName == groupName)
                .Select(t => t.Time)
                .ToListAsync();

            return times;
        }

        public async Task RemoveGroupAsync(string groupName)
        {
            var group = _dbContext.Groups.Where(x => x.GroupName == groupName).SingleOrDefault();

            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();    
        }

        public async Task<int> RemoveTimeAsync(string groupName, string time)
        {
            var g = _dbContext.Groups.Include(t => t.Times).Where(x => x.GroupName == groupName).SingleOrDefault();

            var t = g.Times.Where(x => x.Time == time).SingleOrDefault();

            _dbContext.Times.Remove(t);
            await _dbContext.SaveChangesAsync();
            return 1;
        }

        public async Task<List<string>> GetAllTimes()
        {
            var all =  _dbContext.Groups
                
                .SelectMany(g => g.Times.Select(t => t.Time).ToList());

            return all.ToList();
        }
    }
}
