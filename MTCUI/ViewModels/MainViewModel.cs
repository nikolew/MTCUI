using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Messages.Bluetooth;
using MTCCore.Messages.Master;
using MTCCore.Messages.Nodes;
using MTCCore.Messages.Timer;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Services.Common;
using MTCUI.Services;
using MTCUI.Views;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class MainViewModel : ViewModel
    {
        private bool _initialized;


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

        private readonly IWindowService _windowService;
        private readonly NodeManagerViewModel _nodeManagerViewModel;
        private readonly Clock _clock;

        private DispatcherQueue _dispatcher;


        public MainViewModel(
            GraphViewModel graphViewModel,
            IWindowService windowService, 
            NodeManagerViewModel nodeManagerViewModel,
            Clock clock)
        {
            CurrentView = graphViewModel;
            _windowService = windowService;
            _nodeManagerViewModel = nodeManagerViewModel;
            _clock = clock;
        }

        public async Task InitializeAsync(DispatcherQueue dispatcherQueue, object o)
        {
            if (_initialized)
                return;

            _initialized = true;

            _dispatcher = dispatcherQueue;

            ConnectionStatus = "Опит за свързване...";

            WeakReferenceMessenger.Default.Register<NodeUpdateStatusMessage>(this, (r, m) => UpdateNodeStatus(m.Node));
            WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.NodeEvent));
            WeakReferenceMessenger.Default.Register<BluetoothStatusMessage>(this, (r, m) =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    ConnectionStatus = m.Status;
                });
            });

            WeakReferenceMessenger.Default.Register<TimerTickMessage>(this, (r, m) =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    TimerText = m.Time.ToString(@"hh\:mm\:ss");
                });
            });

            WeakReferenceMessenger.Default.Register<NodeUpdateMessage>(this, (r, m) => UpdateNodeOnGraph(m.Node));
            WeakReferenceMessenger.Default.Register<NodeListRequestMessage>(this, (r, m) => OnNodeListRequest(m.NodeListRequest));
        }

        private void OnNodeListRequest(List<ReadNodeDto> nodeListRequest)
        {
            if (CurrentView is not GraphViewModel graphVM)
                return;

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
                            GroupId = node.GroupId,
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

        private void UpdateNodeOnGraph(NodeModel node)
        {
            if (CurrentView is not GraphViewModel graphVM)
                return;

            foreach (var nodeVm in graphVM.NodesViewModel)
            {
                if (nodeVm.Node.NodeId != node.NodeId)
                    continue;

                graphVM.RemoveNode(nodeVm);

                _dispatcher.TryEnqueue(() =>
                {
                    var nodeViewModel = new NodeViewModel() { Node = node };
                    nodeViewModel.InitTemplateView();
                    graphVM.AddNode(nodeViewModel);
                });
                break;
            }
        }

        private void OnNodeEvent(NodeEventModel value)
        {
            Debug.WriteLine($"Online: {value.Online}");
            var n = CurrentView as GraphViewModel;
            foreach (var node in n.NodesViewModel)
            {
                if (node.Node.NodeId == value.Id)
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

        [RelayCommand]
        void Load()
        {
            if (CurrentView is GraphViewModel graphVM)
            {
                graphVM.ClearNodes();
            }
            WeakReferenceMessenger.Default.Send(new MasterCommandMessage(1));
        }

        [RelayCommand]
        void AddNode()
        {
            if (CurrentView is GraphViewModel graphVM)
            {
                var node = new NodeModel
                {
                    Position = new Windows.Foundation.Point(100, 100),
                    TargetType = TargetType.Target8,
                    State = TargetState.TargetFolded,
                    NodeId = 4
                };

                var nodeViewModel = new NodeViewModel() { Node = node };
                nodeViewModel.InitTemplateView();

                graphVM.AddNode(nodeViewModel);
            }
        }

        [RelayCommand]
        void ConfigNode()
        {
            _windowService.OpenWindow<NodeManagerWindow>(null);
        }

        [RelayCommand]
        void SaveScene()
        {
            var gr = CurrentView as GraphViewModel;
            var nodes = gr.NodesViewModel;

            var nodesSave = new List<SaveNodeDto>();

            foreach(var item in nodes)
            {
                nodesSave.Add(new SaveNodeDto
                {
                    UniqueNodeId = item.Node.UniqueNodeId,
                    NodeId = item.Node.NodeId,
                    Position = item.Node.Position,
                    TargetType = item.Node.TargetType,
                    GroupId = item.Node.GroupId,
                });
            }

            WeakReferenceMessenger.Default.Send(new NodeSaveMessage(nodesSave));
        }

        [RelayCommand]
        void ResetNodes() 
        { 
            WeakReferenceMessenger.Default.Send(new NodeSendCommandMessage(255, CommandType.CMD_NODERST));
        }

        [RelayCommand]
        void Scheduler()
        {
            _windowService.OpenWindow<SchedulerWindow>(null);
        }
        
        #region Timer
        [RelayCommand]
        void StartTimer()
        {
            _clock.Start();
        }

        [RelayCommand]
        void StopTimer()
        {
            _clock.Stop();
        }
        [RelayCommand]
        void ResetTimer()
        {
            _clock.Reset();
        }
        #endregion
    }
}
