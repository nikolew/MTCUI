using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Domain.Enums;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Services.Groups;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<GroupModel> _groupModels = new();

        public NodeEditViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            _dispatcher = dispatcher;

            Enabled = true;

            Item = o as ItemModel;

            TargetType = Item.TargetType;

            _dispatcher.TryEnqueue(() =>
            {
               

                var grups = _groupService.GetAllAsync().Result;
                foreach (var dto in grups)
                    _groupModels.Add(new GroupModel(dto));

                
                Groups = _groupModels.Select(x => x.Name).ToList(); ;
                SelectedGroupName = Groups.FirstOrDefault(x => x == "None");
            });

            WeakReferenceMessenger.Default.Register<NodeGetConfigMessage>(this, (r, m) => { OnNodeConfigReceived(m.NodeConfig); });
        }

        private void OnNodeConfigReceived(NodeConfigModel nodeConfig)
        {
            var group = _groupModels.FirstOrDefault(x => x.Id == nodeConfig.GroupId);

            _dispatcher.TryEnqueue(() =>
            {
                //if (nodeConfig.Id != Convert.ToInt32(Item.TargetId))
                //    return;
                Enabled = true;
                SelectedGroupName = group.Name;
                LightMode = nodeConfig.Light;

            });

            //var node = new NodeModel
            //{
            //    UniqueNodeId = Item.UniqueId,
            //    NodeId = Item.NodeId,
            //    GroupId = nodeConfig.GroupId,
            //    TargetType = Item.TargetType,
            //    Distance = Item.Distance,
            //    Position = Item.Position
                
            //};
            //_core.UpdateNode(node);
        }


        [RelayCommand]
       void GetConfig()
        {
            Enabled = false;
            WeakReferenceMessenger.Default.Send(new NodeSendCommandMessage(Item.NodeId, CommandType.CMD_NODEREADCONFIG));
        }


        [RelayCommand]
        void Save()
        {
            Enabled = false;

            var group = _groupModels.FirstOrDefault(x => x.Name == SelectedGroupName);

            var config = new NodeConfigModel
            {
                NodeId = Convert.ToInt32(Item.NodeId),
                GroupId = group.Id,
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
