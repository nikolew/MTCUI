using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Foundation;
using CommunityToolkit.Mvvm.Input;
using MTCCore.Domain.Enums;

namespace MTCUI.Models
{
    public partial class ItemModel : ObservableObject
    {
        public Action<ItemModel> SaveAction;
        public Action<ItemModel> EditAction;

        private TargetType _origTargetType;
        private int _origGroup;  
        private string _origDistance = "";
        private LightMode _originalLight;
        
        [ObservableProperty]
        private string _uniqueId;

        [ObservableProperty]
        private int _nodeId;

        [ObservableProperty]
        private Point _position;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDirty))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private TargetType _targetType;

        [ObservableProperty]
        private TargetState _state;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDirty))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _distance;

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        private string _rssi;

        [ObservableProperty]
        private string _snr;

        [ObservableProperty]
        private string _battVoltage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDirty))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private LightMode _light;

        [ObservableProperty]
        private string _groupContent;

        private static string Norm(string? s) => (s ?? "").Trim();
        
        public bool IsDirty =>
            TargetType != _origTargetType ||
            GroupId  != _origGroup ||
            Norm(Distance) != _origDistance ||
            Light != _originalLight;
        
        
        public void Load(TargetType target, int group, string? distance)
        {
            _origTargetType = target;
            _origGroup  = group;
            _origDistance = Norm(distance);
            
            TargetType = _origTargetType;
            GroupId = _origGroup;
            Distance = _origDistance;

            GroupContent = Enum.GetName(typeof(Group), GroupId);

            OnPropertyChanged(nameof(IsDirty));
            SaveCommand.NotifyCanExecuteChanged();
        }
        
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            SaveAction.Invoke(this);
            
            _origTargetType = TargetType;
            _origGroup  = GroupId;
            _origDistance = Norm(Distance);
            _originalLight = Light;
            
            Distance = _origDistance;

            OnPropertyChanged(nameof(IsDirty));
            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave() => IsDirty;

        [RelayCommand]
        void Edit()
        {
            EditAction.Invoke(this);
        } 
    }
}
