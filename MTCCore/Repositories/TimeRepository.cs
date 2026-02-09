using MTCCore.DataBase;
using MTCCore.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MTCCore.Repositories
{
    public interface ITimeRepository
    {
        List<TimeEntity> GetAll();
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
    }
}
