using MTCCore.DTO.Grups;
using MTCCore.DTO.Times;
using MTCCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Groups
{
    public interface IGroupService
    {
        Task<List<GroupModel2>> GetAllGroupsAsync();
        Task<int> CreateGroupAsync(string name);


        Task<GroupReadDto> CreateGroupAsync(CreateGroupDto dto);
        Task AddTimeAsync(AddTimeDto dto);
        Task<List<GroupReadDto>> GetAllAsync();
        Task RemoveTimeAsync(RemoveTimeDto dto);
        Task RemoveGroupAsync(RemoveGroupDto dto);
    }
}