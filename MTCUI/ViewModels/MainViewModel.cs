using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using MTCCore.Enums;
using MTCCore.Messages.Bluetooth;
using MTCCore.Messages.Master;
using MTCCore.Messages.Nodes;
using MTCCore.Messages.Timer;
using MTCCore.Models;
using MTCUI.Models;
using MTCUI.Services;
using MTCUI.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MTCCore.Services;
using WinRT.MTCUIVtableClasses;

namespace MTCUI.ViewModels
{
    public partial class MainViewModel : ViewModel
    {
        private bool _initialized;

        private readonly CoreService _core;

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

        private DispatcherQueue _dispatcher;


        public MainViewModel(CoreService core, GraphViewModel graphViewModel,
            IWindowService windowService, NodeManagerViewModel nodeManagerViewModel)
        {
            _core = core;
           


            CurrentView = graphViewModel;


            _windowService = windowService;
            _nodeManagerViewModel = nodeManagerViewModel;
        }

        public async Task InitializeAsync(DispatcherQueue dispatcherQueue)
        {
            if (_initialized)
                return;

            _initialized = true;

            _dispatcher = dispatcherQueue;

            WeakReferenceMessenger.Default.Register<NodeAddToViewGraphMessage>(this, (r, m) => AddNodeToGraph(m.Node));
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

            WeakReferenceMessenger.Default.Register<NodeUpdateMessage>(this, (r, m) => UpdateNode(m.Id));
          
            await ConnectBluetoothAsync();
        }

        private void UpdateNode(string id)
        {
            var nodeModel = _core.GetNodebyUniqueId(id);
            if (CurrentView is not GraphViewModel graphVM) 
                return;
            
            foreach (var nodeVm in graphVM.NodesViewModel)
            {
                if (nodeVm.Node.UniqueId != id) 
                    continue;
                
                graphVM.RemoveNode(nodeVm);
                
                _dispatcher.TryEnqueue(() =>
                {
                    var nodeViewModel = new NodeViewModel() { Node = nodeModel };
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

                var id = Convert.ToString(value.Id);
                if (node.Node.TargetId == id)
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
                if (node.Node.TargetId == value.TargetId)
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
        
        

        private async Task AddNodeToGraph(NodeModel node)
        {
            if (CurrentView is not GraphViewModel graphVM)
                return;

            _dispatcher.TryEnqueue(() =>
            {
                var nodeViewModel = new NodeViewModel() { Node = node };

                nodeViewModel.InitTemplateView();
                graphVM.AddNode(nodeViewModel);
            });
        }

        private async Task ConnectBluetoothAsync()
        {
            ConnectionStatus = "Опит за свързване...";

            WeakReferenceMessenger.Default.Send(new BluetoothConnectMessage());
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
                    TargetId = "4"
                };

                var nodeViewModel = new NodeViewModel() { Node = node };
                nodeViewModel.InitTemplateView();

                graphVM.AddNode(nodeViewModel);
            }
        }

        [RelayCommand]
        void ConfigNode()
        {
            _windowService.OpenWindow<NodeManagerWindow>();
        }

        [RelayCommand]
        void Save()
        {
            var gr = CurrentView as GraphViewModel;
            var nodes = gr.NodesViewModel;

            _core.Save(nodes);
        }

        [RelayCommand]
        void StartTimer()
        {
            _core.StartTimer();
        }

        [RelayCommand]
        void StopTimer()
        {
            _core.StopTimer();

        }
    }
}
