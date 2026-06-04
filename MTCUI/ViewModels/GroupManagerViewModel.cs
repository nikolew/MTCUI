using CommunityToolkit.Mvvm.ComponentModel;
using MTCCore.Services.Groups;

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
