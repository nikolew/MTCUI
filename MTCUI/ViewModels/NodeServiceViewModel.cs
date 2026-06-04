using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeServiceViewModel : ViewModel
    {
        private DispatcherQueue _dispatcher;
        private readonly INodeService _nodeService;

        private static readonly ObservableCollection<NodeInfo> nodesInfo = new();

        [ObservableProperty]
        private ObservableCollection<NodeInfo> _nodes = nodesInfo;

        [ObservableProperty]
        private NodeInfo _selectedNode;

        public NodeServiceViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();

            WeakReferenceMessenger.Default.Register<NodeUpdateStatusMessage>(this, (r, m) => UpdateNodeStatus(m.Node));
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

        private void UpdateNodeStatus(NodeModel node)
        {
            //var n = Nodes.SingleOrDefault(x => x.NodeId == node.NodeId);

            foreach(var n in Nodes)
            {
                if(n.NodeId == node.NodeId)
                {
                    var snr = Convert.ToString(node.Snr);
                    var rssi = Convert.ToString(node.Rssi);

                    _dispatcher.TryEnqueue(() =>
                    {
                        n.Snr = $"{snr} dB";
                        n.Rssi = $"{rssi} dBm";

                        //node.BattVoltage = $"{m.Node.BattVoltage} V";
                    });
                }
            }

        }
    }
}
