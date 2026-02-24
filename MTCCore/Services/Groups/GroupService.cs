using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using MTCCore.DTO.Grups;
using MTCCore.DTO.Times;
using MTCCore.Extensions.Groups;
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

    public async Task<List<GroupModel2>> GetAllGroupsAsync()
    {
        var groups = _dbContext.Groups;

        return [.. groups.Select(group => new GroupModel2
        {
            GroupId = group.Id,
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



    // New method to create a group using a DTO
    public async Task<GroupReadDto> CreateGroupAsync(CreateGroupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Group name is required");

        bool exists = await _dbContext.Groups.AnyAsync(g => g.GroupName == dto.Name);

        if (exists)
            throw new InvalidOperationException("Duplicate group name");

        var entity = new GroupEntity
        {
            GroupName = dto.Name,
            Color = dto.Color
        };

        _dbContext.Groups.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity.ToReadDto();
    }

    // Method to retrieve all groups and convert them to DTOs
    public async Task<List<GroupReadDto>> GetAllAsync()
    {
        var groups = await _dbContext.Groups
            .Include(g => g.Times)
            .AsNoTracking()
            .ToListAsync();

        return groups.Select(g => g.ToReadDto()).ToList();
    }

    // Method to add a time to a group
    public async Task AddTimeAsync(AddTimeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Time))
            throw new ArgumentException("Time is required");

        var group = await _dbContext.Groups
            .Include(g => g.Times)
            .FirstOrDefaultAsync(g => g.Id == dto.GroupId);

        if (group == null)
            throw new InvalidOperationException("Group not found");

        // duplicate guard
        if (group.Times.Any(t => t.Time == dto.Time))
            return;

        group.Times.Add(new TimeEntity
        {
            Time = dto.Time
        });

        await _dbContext.SaveChangesAsync();
    }

    // Method to remove a time from a group
    public async Task RemoveTimeAsync(RemoveTimeDto dto)
    {
        var time = await _dbContext.Times
            .FirstOrDefaultAsync(t =>
                t.GroupEntityId == dto.GroupId &&
                t.Time == dto.Time);

        if (time == null)
            return;

        _dbContext.Times.Remove(time);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveGroupAsync(RemoveGroupDto dto)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.GroupName == dto.GroupName);

        if (group == null)
            return;
        
        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }
}