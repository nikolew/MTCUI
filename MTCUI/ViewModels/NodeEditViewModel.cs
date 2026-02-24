using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Domain.Enums;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Services.Groups;
using MTCUI.Models;
using MTCUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeEditViewModel : ViewModel
    {
        private DispatcherQueue _dispatcher;
        private CoreService _core;

        [ObservableProperty]
        private ItemModel _item;

        [ObservableProperty]
        private TargetType _targetType;

        [ObservableProperty]
        private int _groupId;

        [ObservableProperty]
        private LightMode _lightMode;

        [ObservableProperty]
        private bool _enabled;

        [ObservableProperty]
        private string _selectedGroupName;

        private readonly IGroupService _groupService;

        public Array TargetTypes { get; }= Enum.GetValues<TargetType>();
        public Array LightModes { get; } = Enum.GetValues<LightMode>();

        [ObservableProperty]
        private List<string> _groups = new();

        private List<GroupModel2> _groupModels = new();

        public NodeEditViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            _dispatcher = dispatcher;

            Enabled = true;

            _core = Ioc.Default.GetRequiredService<CoreService>();

            Item = o as ItemModel;

            TargetType = Item.TargetType;

            _dispatcher.TryEnqueue(() =>
            {
                _groupModels = _groupService.GetAllGroupsAsync().Result;
                var te = _groupModels.Select(x => x.GroupName).ToList();
                
                Groups = te;
            });

            WeakReferenceMessenger.Default.Register<NodeGetConfigMessage>(this, (r, m) => { OnNodeConfigReceived(m.NodeConfig); });
        }

        private void OnNodeConfigReceived(NodeConfigModel nodeConfig)
        {
            var group = _groupModels.FirstOrDefault(x => x.GroupId == nodeConfig.GroupId);

            _dispatcher.TryEnqueue(() =>
            {
                //if (nodeConfig.Id != Convert.ToInt32(Item.TargetId))
                //    return;
                Enabled = true;
                SelectedGroupName = group.GroupName;
                LightMode = nodeConfig.Light;

            });

            var node = new NodeModel
            {
                UniqueNodeId = Item.UniqueId,
                NodeId = Item.TargetId,
                GroupId = nodeConfig.GroupId,
                TargetType = Item.TargetType,
                Distance = Item.Distance,
                Position = Item.Position
                
            };
            _core.UpdateNode(node);
        }


        [RelayCommand]
        async void GetConfig()
        {
            Enabled = false;
            var id = Convert.ToInt32(Item.TargetId);

            await _core.SendNodeReadConfig(Convert.ToInt32(Item.TargetId));
        }


        [RelayCommand]
        void Save()
        {
            Enabled = false;

            var group = _groupModels.FirstOrDefault(x => x.GroupName == SelectedGroupName);

            var config = new NodeConfigModel
            {
                NodeId = Convert.ToInt32(Item.TargetId),
                GroupId = group.GroupId,
                Light = LightMode
            };

            WeakReferenceMessenger.Default.Send(new NodeSetConfigMessage(config));
        }


        [RelayCommand]
        void Close()
        {
            
        }
    }
}
