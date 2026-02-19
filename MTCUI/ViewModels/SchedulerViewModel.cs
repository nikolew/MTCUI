using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MTCCore.Models;
using MTCCore.Repositories;
using MTCCore.Services.Groups;
using MTCCore.Services.Scheduling;
using MTCUI.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCUI.ViewModels;

public partial class SchedulerViewModel  : ViewModel
{
    [ObservableProperty]
    private ObservableCollection<GroupModel>  _groups = new();

    [ObservableProperty]
    private ObservableCollection<string> _times = new();

    [ObservableProperty]
    private ObservableCollection<string> _allTimes = new();

    [ObservableProperty]
    private GroupModel _selectedGroup;

    [ObservableProperty]
    private string? _selectedTime;

    [ObservableProperty]
    private string _newTime;

    [ObservableProperty]
    private string _newGroupName;

    private DispatcherQueue _dispatcherQueue;

    private IGroupService _groupService;
    private readonly ISchedulingService _schedulingService;

    [ObservableProperty]
    private bool _controlEnabled;

    [ObservableProperty]
    private bool _btnDeleteEnabled;

    public SchedulerViewModel(IGroupService groupService, ISchedulingService schedulingService)
    {
        _groupService = groupService;
        _schedulingService = schedulingService;
    }

    public async Task InitializeAsync(DispatcherQueue dispatcher)
    {
        _dispatcherQueue = dispatcher;

        ControlEnabled = false;
        BtnDeleteEnabled = false;

        Groups.Clear();
        Times.Clear();
        AllTimes.Clear();

        var groups = await _groupService.GetAllGroupsAsync();

        foreach (var group in groups)
        {
            Groups.Add(new GroupModel
            {
                GroupName = group.GroupName,
                Color = group.Color,
            });
        }

        

        var all =  _schedulingService.GetAllTimes().Result.OrderBy(t => t);
        foreach (var time in all)
        {
            AllTimes.Add(time);
        }
    }

    partial void OnSelectedGroupChanged(GroupModel value)
    {
        _ = LoadTimesAsync(value);
    }

    private async Task LoadTimesAsync(GroupModel value)
    {
        if (value is null)
            return;

        if (value.GroupName == "None")
        {
            ControlEnabled = false;
            Times.Clear();
            return;
        }

        ControlEnabled = true;
        Times.Clear();

        var t = await _schedulingService.GetTimesForGroupAsync(value.GroupName);

        // ако t е List<string>
        foreach (var item in t)
            Times.Add(item);

        Times = new ObservableCollection<string>(Times.OrderBy(t => t));
    }

    partial void OnNewTimeChanged(string value)
    {
        value ??= "";

        // взимаме само цифрите
        var digits = new string(value.Where(char.IsDigit).ToArray());

        // правим HH:mm
        if (digits.Length >= 2)
            digits = digits.Insert(2, ":");

        // максимум 5 символа: "HH:mm"
        if (digits.Length > 5)
            digits = digits.Substring(0, 5);

        if (NewTime != digits)
            NewTime = digits;
    }

    partial void OnSelectedTimeChanged(string value)
    {
        BtnDeleteEnabled = true;
    }

    [RelayCommand]
    public void AddTimeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTime) || SelectedGroup is null)
            return;

        var time = NewTime.Trim();

        _schedulingService.AddTimeToGroupAsync(SelectedGroup.GroupName, time);

        Times.Add(time);
        Times = new ObservableCollection<string>(Times.OrderBy(t => t));

        NewTime = string.Empty;

        AllTimes.Add(time);
        AllTimes = new ObservableCollection<string>(AllTimes.OrderBy(t => t));
    }

    [RelayCommand]
    void DeleteTimeAsync() 
    {
        if (SelectedTime is null)
            return;

         _schedulingService.RemoveTimeAsync(SelectedGroup.GroupName, SelectedTime);

        Times.Remove(SelectedTime);
        BtnDeleteEnabled = false;

        AllTimes.Clear();

        var all =  _schedulingService.GetAllTimes().Result.OrderBy(t => t);
        foreach (var time in all)
        {
            AllTimes.Add(time);
        }
    }

    [RelayCommand]
    async Task AddNewGroup(string groupName)
    {
        if (groupName == null)
            return;

        var group = new GroupModel
        {
            GroupName = groupName,
            Color = "#FFFFFF"
        };

        await _groupService.CreateGroupAsync(groupName);

        Groups.Add(new GroupModel { GroupName = groupName, Color = "#FFFFFF" });

        NewGroupName = string.Empty;
    }

    [RelayCommand]
    void DeleteGroupAsync()
    {
        if (SelectedGroup is null || SelectedGroup.GroupName == "None")
            return;

        _schedulingService.RemoveGroupAsync(SelectedGroup.GroupName);

        Groups.Remove(SelectedGroup);

    }
}