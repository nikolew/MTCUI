using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Scheduling
{
    public interface ISchedulingService
    {
        Task<int> AddTimeToGroupAsync(string groupName, string time);
        Task<List<string>> GetTimesForGroupAsync(string groupName);
        Task RemoveGroupAsync(string groupName);
        Task<int> RemoveTimeAsync(string groupName, string time);
        Task<List<string>> GetAllTimes();
    }
}