using MTCCore.DTO.Grups;
using MTCCore.DTO.Times;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTCCore.Services.Groups
{
    public interface IGroupService
    {
        Task<GroupReadDto> CreateGroupAsync(CreateGroupDto dto);
        Task AddTimeAsync(AddTimeDto dto);
        Task<List<GroupReadDto>> GetAllAsync();
        Task RemoveTimeAsync(RemoveTimeDto dto);
        Task RemoveGroupAsync(RemoveGroupDto dto);
    }
}