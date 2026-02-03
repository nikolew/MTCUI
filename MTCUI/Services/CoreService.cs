using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Enums;
using MTCCore.Models;
using MTCCore.Repositories;
using MTCCore.Services;
using MTCUI.Messages;
using MTCUI.Models;
using MTCUI.Utilities;
using MTCUI.ViewModels;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;

namespace MTCUI.Services
{
    public class CoreService
    {
        public event Action<string> OnClientStatusChanged;

        private readonly ClientSocketService _clientSocket;
        private readonly BluetoothLEService _bluetoothService;
        private readonly INodeService _nodeService;

        public CoreService(ClientSocketService clientSocket, BluetoothLEService bluetoothService, INodeService nodeService)
        {
            _clientSocket = clientSocket;
            _bluetoothService = bluetoothService;
            _nodeService = nodeService;

            _bluetoothService.StatusChanged += s =>
            {
                OnClientStatusChanged?.Invoke(s);
            };

            _bluetoothService.ResponseReceived += BluetoothService_ResponseReceived;

            WeakReferenceMessenger.Default.Register<ClientConnectMessage>(this, (r, m) => { OnTryClientConnect(m.Value); });
            WeakReferenceMessenger.Default.Register<NodeClickMessage>(this, (r, m) => { OnNodeClick(m.Value); });
            WeakReferenceMessenger.Default.Register<CommandMessage>(this, (r, m) => { OnCommand(m.Value); });
        }

        private async void OnCommand(int value)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_GETNODES,
                
            };
            await _bluetoothService.Send(packet);
        }

        private async void OnNodeClick(string value)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_NODECMD,
                Node = new Node
                {
                    NodeId = Convert.ToInt32(value)
                }
            };

            await _bluetoothService.Send(packet);
        }

        private void OnTryClientConnect(string value)
        {
            _bluetoothService.StartDiscovery();
        }

        private void BluetoothService_ResponseReceived(byte[] data)
        {
            var packet = Serializer.Deserialize<Packet>(new MemoryStream(data));

            switch (packet.CommandType)
            {
                case CommandType.CMD_PING:
                   Trace.WriteLine($"Ping response received from MTC-01 {DateTime.Now.TimeOfDay}");
                   break;

                case CommandType.CMD_STATUS:
                  
                    var status = packet.NodeStatus;
                    TargetState state;

                    if (status.Position ==0)
                    {
                        state = TargetState.TargetRaised;
                    }
                    else                     {
                        state = TargetState.TargetFolded;
                    }

                    WeakReferenceMessenger.Default.Send(new UpdateNodeStatusMessage(new NodeModel
                    {
                        TargetId = Convert.ToString(status.Id),
                        State = state
                    }));
                    break;

                case CommandType.CMD_GETNODES:
                    var nodes = packet.NodeList.Nodes;
                    
                    foreach(Node item in nodes)
                    {
                        var nuid = Convert.ToHexString(item.UniqueId);
                        var node = _nodeService.GetNodeByUniqueId(nuid);

                        if (node != null)
                        {
                            WeakReferenceMessenger.Default.Send(new AddNodeToViewGraphMessage(node));
                        }
                        else
                        {
                            _nodeService.AddNode(new NodeModel
                            {
                                UniqueId = nuid,
                                TargetId = Convert.ToString(item.NodeId),
                                Position = new Point(100, 100),
                                TargetType = TargetType.Default,
                                State = TargetState.TargetFolded
                            });

                            var node2 = _nodeService.GetNodeByUniqueId(nuid);
                            WeakReferenceMessenger.Default.Send(new AddNodeToViewGraphMessage(node2));
                        }               
                    }

                    break;
            }
        }

        public void Save(IEnumerable<NodeViewModel> nodes)
        {
            if (nodes == null)
                return;

            var listNodes = new List<NodeModel>();
            listNodes = nodes.Select(n => new NodeModel
            {
                UniqueId = n.Node.UniqueId,
                TargetId = n.Node.TargetId,
                Position = n.Node.Position,
                TargetType = n.Node.TargetType,
                State = n.Node.State
            }).ToList();

            _nodeService.UpdateNodes(listNodes);
        }
    }
}
