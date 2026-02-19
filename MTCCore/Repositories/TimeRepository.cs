using MTCCore.Data;
using MTCCore.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MTCCore.Repositories
{
    public interface ITimeRepository
    {
        List<TimeEntity> GetAll();
        void AddTime(TimeEntity time);
    }

    public class TimeRepository : ITimeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TimeRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<TimeEntity> GetAll()
        {
            return _dbContext.Times.ToList();
        }

        public void AddTime(TimeEntity time)
        {
            _dbContext.Times.Add(time);
        }
    }
}
