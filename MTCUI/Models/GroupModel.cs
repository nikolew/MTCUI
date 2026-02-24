using CommunityToolkit.Mvvm.ComponentModel;
using MTCCore.DTO.Grups;
using System.Collections.ObjectModel;

namespace MTCUI.Models
{
    public partial class GroupModel : ObservableObject
    {
        public int Id { get; }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private ObservableCollection<string> times;

        public GroupModel(GroupReadDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Times = new ObservableCollection<string>(dto.Times);
        }
    }
}
