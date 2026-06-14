using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using MTCCore.DTO.Grups;
using MTCCore.Messages.Groups;
using MTCCore.Services.Groups;
using MTCUI.Models;
using System;
using System.Collections.ObjectModel;

using System.Threading.Tasks;
using Windows.UI;

namespace MTCUI.ViewModels
{
    public partial class GroupManagerViewModel : ViewModel
    {
        private readonly IGroupService _groupService;

        [ObservableProperty]
        private string _newGroupName;

        [ObservableProperty]
        private ObservableCollection<GroupModel> _groups = new();

        [ObservableProperty]
        private GroupModel _selectedGroup;


        [ObservableProperty]
        private Color _colorBrush;

        public GroupManagerViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        public async Task InitializeAsync(DispatcherQueue dispatcher)
        {
            Clear();

            var gr  = _groupService.GetAllAsync().Result;

            ColorBrush = Colors.Gray;

            foreach (var group in gr)
            {
                if(group.Name != "None")
                {
                    Groups.Add(new GroupModel(group));
                }
                    
            }
        }

        public void Clear()
        {
            Groups.Clear();
        }

        [RelayCommand]
        void AddNewGroup()
        {
            if (!string.IsNullOrEmpty(NewGroupName))
            {
               
                var newGroup = new CreateGroupDto { Name = NewGroupName, Color = ColorBrush.ToString() };
                _groupService.CreateGroupAsync(newGroup);
                var groups = _groupService.GetAllAsync().Result;
                
                Clear();
                foreach (var group in groups)
                {
                    if (group.Name != "None") 
                    {
                        Groups.Add(new GroupModel(group));
                    }
                }

                NewGroupName = string.Empty;
                WeakReferenceMessenger.Default.Send(new UpdateGroupMessage());
            }
        }

        [RelayCommand]
        void DeleteGroup()
        {
            if (SelectedGroup is not null)
            {
                _groupService.RemoveGroupAsync(new RemoveGroupDto { GroupName = SelectedGroup.Name});
                Groups.Remove(SelectedGroup);
                SelectedGroup = null;

                WeakReferenceMessenger.Default.Send(new UpdateGroupMessage());
            }
        }
    }
}
