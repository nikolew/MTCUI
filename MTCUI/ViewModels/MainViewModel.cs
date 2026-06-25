using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Messages.Bluetooth;
using MTCCore.Messages.Groups;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Protocol.Events;
using MTCCore.Protocol.Handlers;
using MTCCore.Services.Common;
using MTCCore.Services.Communication;
using MTCCore.Services.Nodes;
using MTCUI.Services;
using MTCUI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace MTCUI.ViewModels
{
    public partial class MainViewModel : ViewModel
    {
        private bool _initialized;
        private int _nodeCounter = 1;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private string _connectionStatus;

        [ObservableProperty]
        private string _connectButtonText;

        [ObservableProperty]
        private object _nodeMangerView;

        [ObservableProperty]
        private Visibility _nodeManagerVisibility;

        [ObservableProperty]
        private string _timerText = "00:00:00";

        // Полета за състоянието на InfoBar
        [ObservableProperty]
        private bool _isInfoBarOpen;

        [ObservableProperty]
        private string _infoBarTitle;

        [ObservableProperty]
        private string _infoBarMessage;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity;

        [ObservableProperty]
        private bool _bluetoothStatus = false;

        // Пазим токена, за да можем да откажем предишното известие, ако потребителят извика ново веднага
        private System.Threading.CancellationTokenSource _notificationCts;


        private readonly IWindowService _windowService;
        private readonly NodeManagerViewModel _nodeManagerViewModel;
        private readonly Clock _clock;
        private readonly IBluetoothProtocolService _bluetooth;
        private readonly INodeService _nodeService;
        private readonly ConfigAckEnvelopeHandler _config;
        private DispatcherQueue _dispatcher;
        private NodeStatusEnvelopeHandler _nodeStatusHandler;


        public MainViewModel(
            GraphViewModel graphViewModel,
            IWindowService windowService, 
            NodeManagerViewModel nodeManagerViewModel,
            Clock clock, IBluetoothProtocolService bluetooth,
            NodeListEnvelopeHandler nodeList, 
            NodeDataEnvelopeHandler nodeData,
            INodeService nodeService,
            ConfigAckEnvelopeHandler config)
        {
            CurrentView = graphViewModel;
            _windowService = windowService;
            _nodeManagerViewModel = nodeManagerViewModel;
            _clock = clock;
            _bluetooth = bluetooth;
            _nodeService = nodeService;
            _config = config;

            var bt = Ioc.Default.GetRequiredService<IBluetoothService>();
            bt.ConnectionStateChanged += Bt_ConnectionStateChanged;

            _nodeStatusHandler = Ioc.Default.GetRequiredService<NodeStatusEnvelopeHandler>();
            _nodeStatusHandler.NodeStatus += NodeStatusReceived;
        }

        public async Task InitializeAsync(DispatcherQueue dispatcherQueue, object o)
        {
            if (_initialized)
                return;

            _initialized = true;
            _dispatcher = dispatcherQueue;
            _config.ConfigAck += Master_ConfigAck;
            ConnectionStatus = "Опит за свързване...";

            WeakReferenceMessenger.Default.Register<BluetoothStatusMessage>(this, (r, m) =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    ConnectionStatus = m.Status;
                });
            });

            WeakReferenceMessenger.Default.Register<NodeListRequestMessage>(this, (r, m) => OnNodeListRequest(m.NodeListRequest));
            WeakReferenceMessenger.Default.Register<NodeUpdateStatusMessage>(this, (r, m) => UpdateNodeStatus(m.Node));

        }


        private void Master_ConfigAck(object sender, ConfigAckEnvelopeEventArgs e)
        {
            _dispatcher.TryEnqueue(async () =>
            {
                 await ShowNotificationAsync("Master", e.ConfigAck.Message, InfoBarSeverity.Success);
            });
        }

        private void NodeStatusReceived(object sender, NodeStatusEnvelopeEventArgs e)
        {
            if (CurrentView is not GraphViewModel n)
                return;

            foreach (var node in n.NodesViewModel)
            {
                if (node.Node.NodeId == e.NodeStatus.NodeId)
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        node.Node.State = TargetState.TargetOffline;
                        node.InitTemplateView();
                    });
                    break;
                }
            }
        }

        private async void Bt_ConnectionStateChanged(object sender, bool status)
        {
            _dispatcher.TryEnqueue(async () =>
            {
                BluetoothStatus = status;
                if (BluetoothStatus)
                {
                     await ShowNotificationAsync("Master", "Установена е връзка!", InfoBarSeverity.Success);
                }
                else
                {
                     await ShowNotificationAsync("Master", "Връзката е прекъсната!", InfoBarSeverity.Error);
                }
            });

            
        }

        private void UpdateNodeStatus(NodeModel value)
        {
            if (CurrentView is not GraphViewModel n)
                return;

            foreach (var node in n.NodesViewModel)
            {
                if (node.Node.NodeId == value.NodeId)
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        node.Node.State = value.State;
                        node.InitTemplateView();
                    });
                    break;
                }
            }
        }

        /// <summary>
        /// метод за показване на известие с автоматично скриване след 5 секунди
        /// </summary>
        public async Task ShowNotificationAsync(string title, string message, InfoBarSeverity severity)
        {
            // 1. Анулираме предишния таймер, ако все още брои
            _notificationCts?.Cancel();
            _notificationCts = new System.Threading.CancellationTokenSource();
            var token = _notificationCts.Token;

            // 2. Задаваме стойностите за нотификацията
            InfoBarTitle = title;
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            IsInfoBarOpen = true;

            try
            {
                // 3. Изчакваме асинхронно 5 секунди, без да блокираме UI нишката
                await Task.Delay(TimeSpan.FromSeconds(5), token);

                // 4. Скриваме InfoBar-а след изтичане на времето
                IsInfoBarOpen = false;
            }
            catch (TaskCanceledException)
            {
                // Методът е бил извикан отново с ново съобщение, затова просто прекратяваме текущото затваряне
            }
        }

        private void OnNodeListRequest(List<ReadNodeDto> nodeListRequest)
        {
            if (CurrentView is not GraphViewModel graphVM)
                return;

            graphVM.ClearNodes();

            foreach (ReadNodeDto node in nodeListRequest)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    var nodeViewModel = new NodeViewModel()
                    {
                        Node = new NodeModel
                        {
                            UniqueNodeId = node.UniqueNodeId,
                            NodeId = node.NodeId,
                            Distance = node.Distance,
                            Position = node.Position,
                            GroupName = node.GroupName,
                            Rssi = node.Rssi,
                            Snr = node.Snr,
                            BattVoltage = node.BattVoltage,
                            TargetType = node.TargetType,
                            State = node.State
                        }
                    };

                    nodeViewModel.InitTemplateView();
                    graphVM.AddNode(nodeViewModel);
                });
            }
        }


        #region RelayCommand

        [RelayCommand]
        async Task Load()
        {
            if (!BluetoothStatus)
            {
                await ShowNotificationAsync("Master", "Няма връзка!", InfoBarSeverity.Error);
                return;
            }

            // var gr = CurrentView as GraphViewModel;
            // gr.ClearNodes();

            _nodeService.LoadScene();
        }

        [RelayCommand]
        void ResetNodes()
        {
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                SendNodeCommandReq = new SendNodeCommandReq
                {
                    NodeId = 0xFE, // broadcast
                    NodeCommand = NodeCommand.CMD_RESET,
                }
            };

            _nodeService.NodeCommand(packet);
        }

        [RelayCommand]
        void SaveScene()
        {
            var gr = CurrentView as GraphViewModel;
            var nodes = gr.NodesViewModel;

            var nodesSave = new List<SaveNodeDto>();

            foreach (var item in nodes)
            {
                nodesSave.Add(new SaveNodeDto
                {
                    UniqueNodeId = item.Node.UniqueNodeId,
                    NodeId = item.Node.NodeId,
                    Position = item.Node.Position,
                    TargetType = item.Node.TargetType,
                    Distance = item.Node.Distance
                });
            }


            _nodeService.SaveScene(nodesSave);
        }

        [RelayCommand]
        void ResetMaster()
        {
            var msg = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                ResetMaster = new ResetMasterReq
                {
                    DelayMs = 5000
                }
            };

            _bluetooth.SendDataAsync(msg);
        }

        [RelayCommand]
        void Scheduler()
        {
            _windowService.OpenWindow<SchedulerWindow>(null);
        }

        [RelayCommand]
        void GroupManager()
        {
            _windowService.OpenWindow<GroupManagerWindow>(null);
        }


        [RelayCommand]
        void ConfigNode()
        {
            _windowService.OpenWindow<NodeManagerWindow>(null);
            //_windowService.OpenWindow<NodeServiceWindow>(null);
        } 
        #endregion

    }
}
