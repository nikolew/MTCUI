using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Domain.Entities;
using MTCCore.Domain.Enums;
using MTCCore.Messages.Bluetooth;
using MTCCore.Messages.Master;
using MTCCore.Messages.Nodes;
using MTCCore.Messages.Timer;
using MTCCore.Models;
using MTCCore.Repositories;
using MTCCore.Services.Common;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using MTCUI.ViewModels;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MTCUI.Services
{
    public class CoreService
    {
        private readonly BluetoothLEService _bluetoothService;
        private readonly ITimeRepository _timeRepository;
        private readonly INodeService _nodeService;
        private readonly Clock _scheduler;
        private readonly IGroupService _groupService;

        private List<TimeEntity> _times;
        private List<GroupModel2> _groups;

        public CoreService(BluetoothLEService bluetoothService, ITimeRepository timeRepository,
            INodeService nodeService, Clock scheduler, IGroupService groupService)
        {

            _bluetoothService = bluetoothService;
            _timeRepository = timeRepository;
            _nodeService = nodeService;
            _scheduler = scheduler;
            _groupService = groupService;

            WeakReferenceMessenger.Default.Register<NodeSendCommandMessage>(this, (r, m) => { OnNodeClick(m.Id); });
            WeakReferenceMessenger.Default.Register<MasterCommandMessage>(this, (r, m) => { OnCommand(m.Command); });
            WeakReferenceMessenger.Default.Register<BluetoothConnectMessage>(this, (r, m) => { OnTryClientConnect(); });
            WeakReferenceMessenger.Default.Register<BluetoothResponseMessage>(this, (r, m) => { OnBluetoothResponse(m.Response); });
            WeakReferenceMessenger.Default.Register<TimerTickMessage>(this, (r, m) => { OnTimerTick(m.Time); });
            WeakReferenceMessenger.Default.Register<NodeSetConfigMessage>(this, (r, m) => { SendNodeConfiguration(m.NodeConfig); });
        }

        private async void OnTimerTick(TimeSpan timeSpan)
        {
            var time = timeSpan.ToString(@"mm\:ss");
            //var group = _groups.SingleOrDefault(g => g.Times.Any(t => t.Time == time));

           // if (group != null)
           // {
           //     await SendGroupCommand(group.Id);
           // }
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

        private void OnTryClientConnect()
        {
            _bluetoothService.StartDiscovery();
        }

        private void OnBluetoothResponse(byte[] data)
        {
            var packet = Serializer.Deserialize<Packet>(new MemoryStream(data));

            switch (packet.CommandType)
            {
                case CommandType.CMD_PING:
                    Trace.WriteLine($"Ping response received from MTC-01 {DateTime.Now.TimeOfDay}");
                    break;

                case CommandType.CMD_STATUS:

                    var status = packet.NodeStatus;
                    TargetState state = TargetState.TargetFolded;

                    if (status.Position == 0 && status.State == 0)
                    {
                        state = TargetState.TargetRaised;
                    }
                    else if (status.Position == 1 && status.State == 0)
                    {
                        state = TargetState.TargetFolded;
                    }
                    else if (status.State == 1)
                    {
                        state = TargetState.TargetHit;
                    }

                    ushort raw = (ushort)(status.BattVoltage[0] | (status.BattVoltage[1] << 8));
                    float voltage = raw / 100.0f;

                    WeakReferenceMessenger.Default.Send(new NodeUpdateStatusMessage(new NodeModel
                    {
                        NodeId = Convert.ToString(status.Id),
                        State = state,
                        Rssi = status.Rssi,
                        Snr = status.Snr,
                        BattVoltage = voltage
                    }));
                    break;

                case CommandType.CMD_GETNODES:
                    var nodes = packet.NodeList.Nodes;

                    foreach (Node item in nodes)
                    {
                        var nuid = Convert.ToHexString(item.UniqueId);
                        var node = _nodeService.GetNodeByUniqueId(nuid);

                        if (node != null)
                        {
                            WeakReferenceMessenger.Default.Send(new NodeAddToViewGraphMessage(node));
                        }
                        else
                        {
                            _nodeService.AddNodeAsync(1, new NodeModel
                            {
                                UniqueNodeId = nuid,
                                NodeId = Convert.ToString(item.NodeId),
                                Position = new Point(100, 100),
                                TargetType = TargetType.Default,
                                State = TargetState.TargetFolded

                            });

                            var node2 = _nodeService.GetNodeByUniqueId(nuid);
                            WeakReferenceMessenger.Default.Send(new NodeAddToViewGraphMessage(node2));
                        }
                    }

                    break;
                case CommandType.CMD_NODEEVENT:
                    {
                        var nodeEvent = packet.NodeEvent;

                        var nodeEventModel = new NodeEventModel
                        {
                            Id = nodeEvent.Id,
                            Online = nodeEvent.Online,
                            MissedFrames = nodeEvent.MissedFrames,
                            LastSeenMs = nodeEvent.LastSeenMs
                        };
                        WeakReferenceMessenger.Default.Send(new NodeEventMessage(nodeEventModel));
                        break;
                    }

                case CommandType.CMD_NODEREADCONFIG:
                    var nodeConfig = packet.NodeConfig;
                    WeakReferenceMessenger.Default.Send(new NodeGetConfigMessage(new NodeConfigModel
                    {
                        NodeId = nodeConfig.Id,
                        Light = (LightMode)nodeConfig.LightMode,
                        GroupId = nodeConfig.GroupId
                    }));
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
                UniqueNodeId = n.Node.UniqueNodeId,
                //TargetId = n.Node.TargetId,
                Position = n.Node.Position,
                TargetType = n.Node.TargetType,
                Distance = n.Node.Distance,
                //Group = n.Node.Group
                //State = n.Node.State
            }).ToList();

            _nodeService.UpdateNodes(listNodes);
        }

        public void StartTimer()
        {
            _groups = _groupService.GetAllGroupsAsync().Result;
            _scheduler.Start();
        }

        public void StopTimer() => _scheduler.Stop();

        public void ResetTimer()
        {
            _scheduler.Reset();
        }

        public NodeModel GetNodebyUniqueId(string uniqueId)
        {
            var node = _nodeService.GetNodeByUniqueId(uniqueId);
            return node ?? null;
        }

        public async Task SendResetNodes()
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_NODERST,
                Node = new Node
                {
                    NodeId = Convert.ToInt32(1)
                }
            };

            await _bluetoothService.Send(packet);
        }

        public async Task SendGroupCommand(int groupId)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_GROUPCMD,
                TargetGroup = new TargetGroup
                {
                    TargetGroupId = groupId
                }
            };
            await _bluetoothService.Send(packet);
        }

        public async Task SendNodeConfiguration(NodeConfigModel node)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_NODESETCONFIG,
                NodeConfig = new NodeConfig
                {
                    Id = node.NodeId,
                    LightMode = (int)node.Light,
                    GroupId = node.GroupId
                }
            };
            await _bluetoothService.Send(packet);
        }

        public async Task SendNodeReadConfig(int nodeId)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_NODEREADCONFIG,
                NodeConfig = new NodeConfig
                {
                    Id = nodeId
                }
            };
            await _bluetoothService.Send(packet);
        }

        public void UpdateNode(NodeModel node)
        {
            _nodeService.UpdateNode(node);
        }
    }
}
