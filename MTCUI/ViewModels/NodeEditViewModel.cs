using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Protocol.Events;
using MTCCore.Protocol.Handlers;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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
        private readonly INodeService _nodeService;
        private readonly NodeConfigEnvelopeHandler _nodeConfig;

        public Array TargetTypes { get; }= Enum.GetValues<TargetType>();
        public Array LightModes { get; } = Enum.GetValues<LightMode>();

        [ObservableProperty]
        private List<string> _groups = new();

        private List<GroupModel> _groupModels = new();

        public NodeEditViewModel(IGroupService groupService, INodeService nodeService, NodeConfigEnvelopeHandler nodeConfig)
        {
            _groupService = groupService;
            _nodeService = nodeService;
            _nodeConfig = nodeConfig;

            _nodeConfig.NodeConfigEnvelopeReceived += NodeConfigEnvelopeReceived;
        }

        

        internal async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            _dispatcher = dispatcher;

            Enabled = true;

            Item = o as ItemModel;
            TargetType = Item.TargetType;

            _dispatcher.TryEnqueue(async () =>
            {
                Groups.Clear();
                _groupModels.Clear();

                var grups = _groupService.GetAllAsync().Result;
                foreach (var dto in grups)
                    _groupModels.Add(new GroupModel(dto));

                
                Groups = _groupModels.Select(x => x.Name).ToList(); ;
                SelectedGroupName = Groups.FirstOrDefault(x => x == "None");

                var node = await _nodeService.GetNodeByUniqueIdAsync(Item.NodeId);


            });

            //WeakReferenceMessenger.Default.Register<NodeGetConfigMessage>(this, (r, m) => { OnNodeConfigReceived(m.NodeConfig); });
        }

        //private void OnNodeConfigReceived(NodeConfigModel nodeConfig)
        //{
        //    var group = _groupModels.FirstOrDefault(x => x.Id == nodeConfig.GroupId);

        //    if (group != null)
        //    {
        //        _dispatcher.TryEnqueue(() =>
        //        {
        //            Enabled = true;
        //            SelectedGroupName = group.Name;
        //            LightMode = nodeConfig.Light;

        //        });

        //        var node = new SaveNodeDto
        //        {
        //            UniqueNodeId = Item.UniqueId,
        //            NodeId = Item.NodeId,
        //            GroupId = nodeConfig.GroupId,
        //            TargetType = Item.TargetType,
        //            Distance = Item.Distance,
        //            Position = Item.Position
        //        };

        //        _nodeService.UpdateNodeAsync(node);
        //    }
        //    else
        //    {
        //        _dispatcher.TryEnqueue(() =>
        //        {
        //            Enabled = true;
        //            SelectedGroupName = "None";
        //            LightMode = nodeConfig.Light;

        //        });

        //        var node = new SaveNodeDto
        //        {
        //            UniqueNodeId = Item.UniqueId,
        //            NodeId = Item.NodeId,
        //            GroupId = 1,
        //            GroupName = SelectedGroupName,
        //            TargetType = Item.TargetType,
        //            Distance = Item.Distance,
        //            Position = Item.Position
        //        };

        //        _nodeService.UpdateNodeAsync(node);
        //    }
        //}

        private void NodeConfigEnvelopeReceived(object sender, NodeConfigEnvelopeEventArgs e)
        {
            var config = e.NodeConfig;
            var group = _groupModels.FirstOrDefault(x => x.Id == config.GroupId);

            if (group != null)
            {
                _dispatcher.TryEnqueue(async () =>
                {
                    Enabled = true;
                    SelectedGroupName = group.Name;
                    LightMode = Enum.GetValues<LightMode>()[config.Light];
                });

                var node = new SaveNodeDto
                {
                    UniqueNodeId = Item.UniqueId,
                    NodeId = Item.NodeId,
                    GroupId = config.GroupId,
                    TargetType = Item.TargetType,
                    Distance = Item.Distance,
                    Position = Item.Position,
                };

                _nodeService.UpdateNodeAsync(node);
            }
            else
            {

            }
        }


        [RelayCommand]
       void GetConfig()
        {
            Enabled = false;
         
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                SendNodeCommandReq = new SendNodeCommandReq
                {
                    NodeId = Item.NodeId,
                    NodeCommand = NodeCommand.CMD_READCONFIG,
                }
            };

            _nodeService.NodeCommand(packet);
        }


        [RelayCommand]
        void Save()
        {
            Enabled = false;

            var group = _groupModels.FirstOrDefault(x => x.Name == SelectedGroupName);
            var gr = Convert.ToByte(group.Id);
            var lm = Convert.ToByte(LightMode);
            byte[] b = { gr, lm };

            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                SendNodeCommandReq = new SendNodeCommandReq
                {
                    NodeId = Item.NodeId,
                    NodeCommand = NodeCommand.CMD_CONFIG,
                    Param = b
                }
            };

            _nodeService.NodeCommand(packet);
        }


        [RelayCommand]
        void Close()
        {
            
        }
    }
}
