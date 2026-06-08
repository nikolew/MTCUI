using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using MTCUI.Services;
using MTCUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeManagerViewModel : ViewModel
    {
        private readonly IWindowService _windowService;
        private readonly INodeService _nodeService;
        private DispatcherQueue _dispatcher;

        public Array TargetTypes { get; } = Enum.GetValues(typeof(TargetType));
        public Array TargetGroups { get; } = Enum.GetValues(typeof(Group));
        public Array LightMode { get; } = Enum.GetValues(typeof(LightMode));

        [ObservableProperty]
        private bool _isDirty;
        
        [ObservableProperty]
        private bool _buttonSaveEnabled;

        private static readonly ObservableCollection<ItemModel> itemModels = new();
        private readonly IGroupService _groupService;

        [ObservableProperty]
        private ObservableCollection<ItemModel> _items = itemModels;

        [ObservableProperty]
        private List<string> _groups = new();

        [ObservableProperty]
        private ItemModel _selectedItemModel;

        public NodeManagerViewModel(IGroupService groupService)
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
            _windowService = Ioc.Default.GetRequiredService<IWindowService>();
            _groupService = groupService;

            WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.NodeEvent));
            WeakReferenceMessenger.Default.Register<NodeUpdateStatusMessage>(this, (r, m) => 
            {
                var node = Items.SingleOrDefault(x => x.NodeId == m.Node.NodeId);
                if (node != null)
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        node.Snr = $"{m.Node.Snr} dB";
                        node.Rssi = $"{m.Node.Rssi} dBm";
                        node.Status = "online";
                        node.BattVoltage = $"{m.Node.BattVoltage}V  {m.Node.BattSoc}%";
                    });
                }
            });
            
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher)
        {
            try
            {
                _dispatcher = dispatcher;

                ButtonSaveEnabled = false;

                _dispatcher.TryEnqueue(() =>
                {
                    var t  = _groupService.GetAllAsync().Result;
                    var te = t.Select(x=>x.Name).ToList();
                    Groups = te;
                });


                await _nodeService.GetAllAsync().ContinueWith(task =>
                {
                    var nodes = task.Result;
                    _dispatcher.TryEnqueue(() =>
                    {
                        foreach (var node in nodes)
                        {
                            var item = new ItemModel
                            {
                                UniqueId = node.UniqueNodeId,
                                Position = node.Position,
                                NodeId = node.NodeId,
                                Status = "offline",
                                GroupName = node.GroupName,
                               
                            };
                            item.SaveAction += OnSaveAction;
                            item.EditAction += OnEditAction;
                            item.Load(node.TargetType, node.GroupName, node.Distance);
                            Items.Add(item);
                        }
                    });
                });
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Error initializing NodeManagerViewModel: {ex.Message}");
            }
        }

        private void OnEditAction(ItemModel model)
        {
            _windowService.OpenWindow<NodeEditWindow>(model);
        }

        private void OnNodeEvent(NodeEventModel value)
        {
            var node = Items.Where(x => x.NodeId == value.Id).SingleOrDefault();

            if (node == null)
                return;
            
            node.Status = value.Online ? "online" : "offline";
        }

        [RelayCommand]
        void Save()
        {
        //    var nodesToSave = new List<NodeModel>();
        //    foreach (var node in Items) { 
            
        //        nodesToSave.Add(new NodeModel
        //        {
        //            UniqueNodeId = node.UniqueId,
        //            TargetType = node.TargetType,
        //            Position = node.Position,
        //            NodeId = node.NodeId,
        //            Distance = node.Distance,
        //            GroupId = node.GroupId
        //        });
        //    }

            //_nodeService.UpdateNodesAsync(nodesToSave);
        }

        internal void Clear()
        {
            Items.Clear();
        }

        private void OnSaveAction(ItemModel node)
        {
            var dto = new SaveNodeDto
            {
                UniqueNodeId = node.UniqueId,
                TargetType = node.TargetType,
                Position = node.Position,
                NodeId = node.NodeId,
                Distance = node.Distance,
                GroupName = node.GroupName
            };

            _nodeService.UpdateNodeAsync(dto);

            var updateNode = new NodeModel
            {
                UniqueNodeId = node.UniqueId,
                NodeId = node.NodeId,
                TargetType= node.TargetType,
                Position = node.Position
            };

            WeakReferenceMessenger.Default.Send(new NodeUpdateMessage(updateNode));
        }
    }
}
