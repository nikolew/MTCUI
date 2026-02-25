using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MTCCore.DTO.Grups;
using MTCCore.DTO.Times;
using MTCCore.Services.Groups;
using MTCUI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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


    [ObservableProperty]
    private bool _controlEnabled;

    [ObservableProperty]
    private bool _btnDeleteEnabled;

    public SchedulerViewModel(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public async Task InitializeAsync(DispatcherQueue dispatcher)
    {
        _dispatcherQueue = dispatcher;

        ControlEnabled = false;
        BtnDeleteEnabled = false;

        Groups.Clear();
        Times.Clear();
        AllTimes.Clear();

        var data = await _groupService.GetAllAsync();
        foreach (var dto in data)
            Groups.Add(new GroupModel(dto));

        await LoadAllTimes();
    }

    partial void OnSelectedGroupChanged(GroupModel value)
    {
        _ = LoadTimesAsync(value);
    }

    private Task LoadTimesAsync(GroupModel group)
    {
        if (group is null)
            return Task.CompletedTask;

        if (group.Name == "None")
        {
            ControlEnabled = false;
            Times.Clear();
            return Task.CompletedTask;
        }

        ControlEnabled = true;
        Times.Clear();

        var data =  _groupService.GetAllAsync().Result;
        var t = data.FirstOrDefault(g => g.Id == group.Id);
        
        foreach (var item in t.Times)
            Times.Add(item);

        Times = new ObservableCollection<string>(Times.OrderBy(t => t));
        return Task.CompletedTask;
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

    private async Task LoadAllTimes()
    {
        var data = await _groupService.GetAllAsync();
        var times = data.SelectMany(g => g.Times).Distinct().OrderBy(t => t);
        foreach (var item in times)
        {
            AllTimes.Add(item);
        }
    }
    
    [RelayCommand]
    async void AddTimeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTime))
           return;

        var time = NewTime.Trim();

        if (string.IsNullOrWhiteSpace(NewTime))
            return;

        await _groupService.AddTimeAsync(new AddTimeDto
        {
            GroupId = SelectedGroup.Id,
            Time = NewTime
        });

        Times.Add(time);
        Times = new ObservableCollection<string>(Times.OrderBy(t => t));
        
        NewTime = string.Empty;
        
        AllTimes.Add(time);
        AllTimes = new ObservableCollection<string>(AllTimes.OrderBy(t => t));
    }

    [RelayCommand]
    async void DeleteTimeAsync() 
    {
        await _groupService.RemoveTimeAsync(new RemoveTimeDto
        {
            GroupId = SelectedGroup.Id,
            Time = SelectedTime
        });

        SelectedGroup.Times.Remove(SelectedTime);
        BtnDeleteEnabled = false;

        Times.Remove(SelectedTime);
        AllTimes.Clear();

        await LoadAllTimes();
    }

    [RelayCommand]
    async Task AddNewGroup(string groupName)
    {
        var dto = new CreateGroupDto { Name = NewGroupName, Color = "#FFFFFF" };

        var created = await _groupService.CreateGroupAsync(dto);

        Groups.Add(new GroupModel(created));

        NewGroupName = string.Empty;
    }

    [RelayCommand]
    void DeleteGroupAsync()
    {
        if (SelectedGroup is null || SelectedGroup.Name == "None")
            return;
        
       _groupService.RemoveGroupAsync(new RemoveGroupDto
       {
           GroupName = SelectedGroup.Name
       });
       
       Groups.Remove(SelectedGroup);
    }
}