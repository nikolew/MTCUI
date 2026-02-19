using MTCCore.Data;
using MTCCore.Domain.Entities;
using MTCCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTCCore.Services.Groups;


public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _dbContext;

    public GroupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GroupModel>> GetAllGroupsAsync()
    {
        var groups = _dbContext.Groups;

        return [.. groups.Select(group => new GroupModel
        {
            GroupName = group.GroupName,
            Color = group.Color
        })];
    }

    public async Task<int> CreateGroupAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required");

        var group = new GroupEntity
        {
            GroupName = name,
            Color = "#FFFFFF" // Default color, can be changed later
        };

        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        return group.Id;
    }
}