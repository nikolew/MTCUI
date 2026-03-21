using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Dispatching;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeServiceViewModel : ViewModel
    {
        private DispatcherQueue _dispatcher;
        private readonly INodeService _nodeService;

        private static readonly ObservableCollection<NodeInfo> nodesInfo= new();

        [ObservableProperty]
        private ObservableCollection<NodeInfo> _nodes = nodesInfo;

        [ObservableProperty]
        private NodeInfo _selectedNode;

        public NodeServiceViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
        }

        public async Task InitializeAsync(DispatcherQueue dispatcher)
        {
            _dispatcher = dispatcher;

            await _nodeService.GetAllAsync().ContinueWith(task =>
            {
                var nodeResult = task.Result;
                _dispatcher.TryEnqueue(() =>
                {
                    foreach (var node in nodeResult)
                    {
                        var nodeInfo = new NodeInfo
                        {
                            UniqueId   = node.UniqueNodeId,
                            NodeId = node.NodeId,
                            Rssi = $"{node.Rssi.ToString()} dBm",
                            Snr = $"{node.Snr.ToString()} dB",
                            BattVoltage = node.BattVoltage.ToString(),
                            Group = node.GroupName,
                            TargetType = node.TargetType.ToString()
                        };

                        Nodes.Add(nodeInfo);
                    }
                });
            });
        }
    }
}
