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
using Group = MTCCore.Domain.Enums.Group;

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
        private Group _group;

        [ObservableProperty]
        private LightMode _lightMode;

        [ObservableProperty]
        private bool _enabled;


        private readonly IGroupService _groupService;

        public Array TargetTypes { get; }= Enum.GetValues<TargetType>();
        public Array TargetGroups { get; } = Enum.GetValues<Group>();
        public Array LightModes { get; } = Enum.GetValues<LightMode>();

        [ObservableProperty]
        private List<string> _groups = new();

        public NodeEditViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            _dispatcher = dispatcher;

            Enabled = false;

            _core = Ioc.Default.GetRequiredService<CoreService>();

            Item = o as ItemModel;

            TargetType = Item.TargetType;

            _dispatcher.TryEnqueue(() =>
            {
                var t = _groupService.GetAllGroupsAsync().Result;
                var te = t.Select(x => x.GroupName).ToList();
                Groups = te;
            });

            WeakReferenceMessenger.Default.Register<NodeGetConfigMessage>(this, (r, m) => { OnNodeConfigReceived(m.NodeConfig); });
        }

        private void OnNodeConfigReceived(NodeConfigModel nodeConfig)
        {
            _dispatcher.TryEnqueue(() =>
            {
                //if (nodeConfig.Id != Convert.ToInt32(Item.TargetId))
                //    return;
                Enabled = true;
                Group = nodeConfig.Group;
                LightMode = nodeConfig.Light;

            });

            var node = new NodeModel
            {
                UniqueId = Item.UniqueId,
                TargetId = Item.TargetId,
                Group = nodeConfig.Group,
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
            var config = new NodeConfigModel
            {
                Id = Convert.ToInt32(Item.TargetId),
                Group = Group,
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
