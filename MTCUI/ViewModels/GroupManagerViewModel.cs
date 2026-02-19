using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTCCore.Models;
using MTCCore.Services.Groups;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCUI.ViewModels
{
    public partial class GroupManagerViewModel : ViewModel
    {
        private readonly IGroupService _groupService;

        [ObservableProperty]
        private string _newGroupName;

        public GroupManagerViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        
    }
}
