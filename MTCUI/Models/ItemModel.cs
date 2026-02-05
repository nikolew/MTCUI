using CommunityToolkit.Mvvm.ComponentModel;
using MTCCore.Enums;
using Windows.Foundation;

namespace MTCUI.Models
{
    public partial class ItemModel : ObservableObject
    {
        [ObservableProperty]
        private string _uniqueId;

        [ObservableProperty]
        private string _targetId;

        [ObservableProperty]
        private Point _position;

        [ObservableProperty]
        private TargetType _targetType;

        [ObservableProperty]
        private TargetState _state;

        [ObservableProperty]
        private string _distance;

        [ObservableProperty]
        private string _status;
    }

}
