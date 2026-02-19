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
        private Group _origGroup;  
        private string _origDistance = "";
        private LightMode _originalLight;
        
        [ObservableProperty]
        private string _uniqueId;

        [ObservableProperty]
        private string _targetId;

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
        //[NotifyPropertyChangedFor(nameof(IsDirty))]
        //[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private Group _group;

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
            Group  != _origGroup ||
            Norm(Distance) != _origDistance ||
            Light != _originalLight;
        
        
        public void Load(TargetType target, Group group, string? distance)
        {
            _origTargetType = target;
            _origGroup  = group;
            _origDistance = Norm(distance);
            
            TargetType = _origTargetType;
            Group  = _origGroup;
            Distance = _origDistance;

            GroupContent = Enum.GetName(typeof(Group), Group);

            OnPropertyChanged(nameof(IsDirty));
            SaveCommand.NotifyCanExecuteChanged();
        }
        
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            SaveAction.Invoke(this);
            
            _origTargetType = TargetType;
            _origGroup  = Group;
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
