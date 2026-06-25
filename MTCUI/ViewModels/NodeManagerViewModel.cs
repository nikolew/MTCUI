using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Protocol.Events;
using MTCCore.Protocol.Handlers;
using MTCCore.Services.Communication;
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
using Windows.UI;

namespace MTCUI.ViewModels
{
    public partial class NodeManagerViewModel : ViewModel
    {
        private readonly IWindowService _windowService;
        private readonly INodeService _nodeService;
        private readonly IBluetoothProtocolService _bluetooth;

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

        private NodeStatusEnvelopeHandler _nodeStatus;

        public NodeManagerViewModel(IGroupService groupService)
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
            _windowService = Ioc.Default.GetRequiredService<IWindowService>();
            _groupService = groupService;
            _nodeStatus = Ioc.Default.GetRequiredService<NodeStatusEnvelopeHandler>();
            _bluetooth = Ioc.Default.GetRequiredService<IBluetoothProtocolService>();

            //WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.NodeEvent));
            WeakReferenceMessenger.Default.Register<NodeUpdateStatusMessage>(this, (r, m) => 
            {
                var node = Items.SingleOrDefault(x => x.NodeId == m.Node.NodeId);
                if (node != null)
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        node.VoltageColor = m.Node.BattVoltage < 10.4 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
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

                _nodeStatus.NodeStatus += OnNodeStatus;

                await _nodeService.GetAllAsync().ContinueWith(task =>
                {
                    var nodes = task.Result;
                    _dispatcher.TryEnqueue(() =>
                    {
                        foreach (var node in nodes)
                        {
                            var groupColor = _groupService.GetColorGrupByName(node.GroupName).Result;
                            var item = new ItemModel
                            {
                                UniqueId = node.UniqueNodeId,
                                Position = node.Position,
                                NodeId = node.NodeId,
                                Status = "offline",
                                GroupName = node.GroupName,
                                VoltageColor = new SolidColorBrush(Colors.Green),
                                GroupColor = new SolidColorBrush(groupColor)
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

        private void OnNodeStatus(object sender, NodeStatusEnvelopeEventArgs e)
        {
            _dispatcher.TryEnqueue(() =>
            {
                var node = Items.Where(x => x.NodeId == e.NodeStatus.NodeId).FirstOrDefault();

                //node.Status = "сдсда";
            });
        }

        private void OnEditAction(ItemModel model)
        {
            _windowService.OpenWindow<NodeEditWindow>(model);
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

        [RelayCommand]
        void Discovery()
        {
            var msg = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                StartDicovery = new StartDiscoveryReq
                {
                    Cicles = 10
                }
            };

            _bluetooth.SendDataAsync(msg);
        }

        [RelayCommand]
        void UpdateNodeStatus()
        {
            var msg = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                GetAllStatus = new GetAllStatusReq
                {
                    
                }
            };

            _bluetooth.SendDataAsync(msg);
        }


        internal void Clear()
        {
            Items.Clear();
        }

        private void OnSaveAction(ItemModel node)
        {
            var grupId = _groupService.GetGrupIdByName(node.GroupName).Result;
            var dto = new SaveNodeDto
            {
                UniqueNodeId = node.UniqueId,
                TargetType = node.TargetType,
                Position = node.Position,
                NodeId = node.NodeId,
                Distance = node.Distance,
                GroupId = grupId
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
