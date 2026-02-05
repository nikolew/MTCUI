using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using MTCCore.Enums;
using MTCCore.Models;
using MTCUI.Messages;
using MTCUI.Models;
using MTCUI.Services;
using MTCUI.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private readonly IWindowService _windowService;
        private readonly NodeManagerViewModel _nodeManagerViewModel;

        private DispatcherQueue _dispatcher;


        public MainViewModel(CoreService core, GraphViewModel graphViewModel, IWindowService windowService, NodeManagerViewModel nodeManagerViewModel)
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

            WeakReferenceMessenger.Default.Register<AddNodeToViewGraphMessage>(this, (r, m) => AddNodeToGraph(m.Value));
            WeakReferenceMessenger.Default.Register<UpdateNodeStatusMessage>(this, (r, m) => UpdateNodeStatus(m.Value));
            WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.Value));

            await ConnectBluetoothAsync();
        }

        private void OnNodeEvent(NodeEvent value)
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
            var n = CurrentView as GraphViewModel;
            foreach(var node in n.NodesViewModel) 
            {
                if(node.Node.TargetId == value.TargetId)
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

            _core.OnClientStatusChanged += (status) =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    ConnectionStatus = status;
                });
                Debug.WriteLine(status);
            };
            WeakReferenceMessenger.Default.Send(new ClientConnectMessage(""));
        }

        [RelayCommand]
        void Load()
        {
            WeakReferenceMessenger.Default.Send(new CommandMessage(1));
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

                //var node2 = new NodeModel
                //{
                //    Position = new Windows.Foundation.Point(200, 100),
                //    TargetType = TargetType.Target10,
                //    State = TargetState.TargetFolded,
                //    TargetId = "1"
                //};

                //var nodeViewModel2 = new NodeViewModel() { Node = node2 };
                //nodeViewModel2.InitTemplateView();
                //graphVM.AddNode(nodeViewModel2);

                //var node3 = new NodeModel
                //{
                //    Position = new Windows.Foundation.Point(300, 100),
                //    TargetType = TargetType.Target10A,
                //    State = TargetState.TargetRaised,
                //    TargetId = "2"
                //};

                //var nodeViewModel3 = new NodeViewModel() { Node = node3 };
                //nodeViewModel3.InitTemplateView();
                //graphVM.AddNode(nodeViewModel3);

                //var node4 = new NodeModel
                //{
                //    Position = new Windows.Foundation.Point(400, 100),
                //    TargetType = TargetType.Default,
                //    State = TargetState.TargetRaised,
                //    TargetId = "3"
                //};

                //var nodeViewModel4 = new NodeViewModel() { Node = node4 };
                //nodeViewModel4.InitTemplateView();
                //graphVM.AddNode(nodeViewModel4);
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
            var gr  = CurrentView as GraphViewModel;
            var nodes = gr.NodesViewModel;

            _core.Save(nodes);
        }

    }
}
