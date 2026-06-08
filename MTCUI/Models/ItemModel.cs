using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using MTCCore.Domain.Enums;
using System;
using Windows.Foundation;


namespace MTCUI.Models
{
    public partial class ItemModel : ObservableObject
    {
        public Action<ItemModel> SaveAction;
        public Action<ItemModel> EditAction;

        private TargetType _origTargetType;
        private string _origGroup;  
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
        private string _groupName;

        [ObservableProperty]
        private string _rssi;

        [ObservableProperty]
        private string _snr;

        [ObservableProperty]
        private string _battVoltage;

        [ObservableProperty]
        private string _battSoc;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDirty))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private LightMode _light;

        [ObservableProperty]
        private string _groupContent;

        [ObservableProperty]
        private Brush _voltageColor;

        [ObservableProperty]
        private Brush _groupColor;

        private static string Norm(string? s) => (s ?? "").Trim();
        
        public bool IsDirty =>
            TargetType != _origTargetType ||
            //GroupName  != _origGroup ||
            Norm(Distance) != _origDistance ||
            Light != _originalLight;

        //public string GroupName { get; internal set; }

        public void Load(TargetType target, string groupName, string? distance)
        {
            _origTargetType = target;
            //_origGroup  = groupName;
            _origDistance = Norm(distance);
            
            TargetType = _origTargetType;
           // GroupName = _origGroup;
            Distance = _origDistance;

            //GroupContent = Enum.GetName(typeof(Group), GroupId);

            OnPropertyChanged(nameof(IsDirty));
            SaveCommand.NotifyCanExecuteChanged();
        }
        
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            SaveAction.Invoke(this);
            
            _origTargetType = TargetType;
            //_origGroup  = GroupId;
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
