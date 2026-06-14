using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using MTCCore.DTO.Grups;
using System;
using System.Collections.ObjectModel;

namespace MTCUI.Models
{
    public partial class GroupModel : ObservableObject
    {
        public Action<GroupModel> SelectGroupAction;

        public int Id { get; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private SolidColorBrush _groupColor;

        [ObservableProperty]
        private ObservableCollection<string> times;

        public GroupModel(GroupReadDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Times = new ObservableCollection<string>(dto.Times);
            GroupColor = new SolidColorBrush(dto.color);
        }

        [RelayCommand]
        void SelectGroup()
        {
            SelectGroupAction.Invoke(this);
        }

        
    }
}
