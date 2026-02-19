using MTCCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Groups
{
    public interface IGroupService
    {
        Task<List<GroupModel>> GetAllGroupsAsync();
        Task<int> CreateGroupAsync(string name);
    }
}