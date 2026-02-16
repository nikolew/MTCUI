using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using MTCCore.Enums;
using MTCUI.Models;
using System;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeEditViewModel : ViewModel
    {
        private DispatcherQueue _dispatcher;

        [ObservableProperty]
        private ItemModel _item;

        [ObservableProperty]
        private TargetType _targetType;

        [ObservableProperty]
        private Group _group;

        [ObservableProperty]
        private LightMode _lightMode;


        public Array TargetTypes { get; } = Enum.GetValues<TargetType>();
        public Array TargetGroups { get; } = Enum.GetValues<Group>();
        public Array LightModes { get; } = Enum.GetValues<LightMode>();

        internal async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            _dispatcher = dispatcher;

            Item = o as ItemModel;  

            TargetType = Item.TargetType;
            Group = Item.Group;
            LightMode = Item.Light;
        }


        [RelayCommand]
        void Save()
        {
            var group = _group;
            var lightMode = _lightMode;

        }
    }
}
