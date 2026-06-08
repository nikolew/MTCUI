using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using MTCCore.DTO.Grups;
using MTCCore.DTO.Times;
using MTCCore.Extensions.Groups;
using MTCCore.Messages.Timer;
using MTCCore.Protocol;
using MTCCore.Services.Communication;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Color = Windows.UI.Color;



namespace MTCCore.Services.Groups;

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBluetoothProtocolService _bluetoothProtocol;

    private List<GroupReadDto> _groups = new();

    public GroupService(ApplicationDbContext dbContext, IBluetoothProtocolService bluetoothProtocol)
    {
        _dbContext = dbContext;
        _bluetoothProtocol = bluetoothProtocol;

        WeakReferenceMessenger.Default.Register<TimerTickMessage>(this, this.OnTimerTick);

        ReloadGroups();
    }

    // Timer tick handler to check if any group has a matching time and send the command
    private async void OnTimerTick(object recipient, TimerTickMessage timeSpan)
    {
        var time = timeSpan.Time.ToString(@"mm\:ss");

        var group = _groups.SingleOrDefault(g => g.Times.Any(t => t == time));
        if (group is not null)
        {
             await SendGroupCommand(group.Id);
        }
    }

    // Helper method to reload groups from the database
    private void ReloadGroups()
    {
        _groups = GetAllAsync().Result;
    }

    // Method to send a command to a group via Bluetooth
    public async Task SendGroupCommand(int groupId)
    {
        var packet = new Envelope
        {
            Seq = 1,
            TsMs = (uint)Environment.TickCount,
            SendNodeCommandReq = new SendNodeCommandReq
            {
                NodeId = 0xFE, // broadcast
                NodeCommand = NodeCommand.CMD_GROUP,
                Param = BitConverter.GetBytes(groupId)
            }
        };

        await _bluetoothProtocol.SendDataAsync(packet);
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

        ReloadGroups();

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

        ReloadGroups();
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

        ReloadGroups() ;
    }

    // Method to remove a group
    public async Task RemoveGroupAsync(RemoveGroupDto dto)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.GroupName == dto.GroupName);

        if (group == null)
            return;
        
        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync();

        ReloadGroups();
    }

    public async Task<int> GetGrupIdByName(string name)
    {
        var id = await _dbContext.Groups
            .Where(g => g.GroupName == name)
            .Select(g => g.Id)
            .FirstOrDefaultAsync();
        return id;
    }

    public async Task<Color> GetColorGrupByName(string name)
    {
        var color = await _dbContext.Groups
            .Where(g => g.GroupName == name)
            .Select(g => g.GroupColor)
            .FirstOrDefaultAsync();

        var t = Color.FromArgb(color.A, color.R, color.G, color.B);
        return t;
    }
}