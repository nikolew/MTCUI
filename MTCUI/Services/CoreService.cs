
using ABI.System;
using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Entities;
using MTCCore.Enums;
using MTCCore.Messages.Bluetooth;
using MTCCore.Messages.Master;
using MTCCore.Messages.Nodes;
using MTCCore.Messages.Timer;
using MTCCore.Models;
using MTCCore.Repositories;
using MTCCore.Services;
using MTCUI.Models;
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
        private readonly BluetoothLEService _bluetoothService;
        private readonly ITimeRepository _timeRepository;
        private readonly INodeService _nodeService;
        private readonly SchedulerService _scheduler;
        private readonly IGroupRepository _groupRepository;
        private List<TimeEntity> _times;

        public CoreService(BluetoothLEService bluetoothService, ITimeRepository timeRepository,
            INodeService nodeService, SchedulerService scheduler, IGroupRepository groupRepository)
        {
           
            _bluetoothService = bluetoothService;
            _timeRepository = timeRepository;
            _nodeService = nodeService;
            _scheduler = scheduler;
            _groupRepository = groupRepository;

            WeakReferenceMessenger.Default.Register<NodeSendCommandMessage>(this, (r, m) => { OnNodeClick(m.Id); });
            WeakReferenceMessenger.Default.Register<MasterCommandMessage>(this, (r, m) => { OnCommand(m.Command); });
            WeakReferenceMessenger.Default.Register<BluetoothConnectMessage>(this, (r, m) => { OnTryClientConnect(); });
            WeakReferenceMessenger.Default.Register<BluetoothResponseMessage>(this, (r, m) => { OnBluetoothResponse(m.Response); });
            WeakReferenceMessenger.Default.Register<TimerTickMessage>(this, (r, m) => { OnTimerTick(m.Time); });

        }

        private void OnTimerTick(System.TimeSpan timeSpan)
        {
            var time = timeSpan.ToString(@"mm\:ss");

            var g = _times.Where(t => t.Time == time).SingleOrDefault();

            if (g != null)
            {
                var n = _nodeService.GetAllNodes();
            }
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
                    TargetState state;

                    if (status.Position == 0)
                    {
                        state = TargetState.TargetRaised;
                    }
                    else                     {
                        state = TargetState.TargetFolded;
                    }

                    WeakReferenceMessenger.Default.Send(new NodeUpdateStatusMessage(new NodeModel
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
                            WeakReferenceMessenger.Default.Send(new NodeAddToViewGraphMessage(node));
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
                            WeakReferenceMessenger.Default.Send(new NodeAddToViewGraphMessage(node2));
                        }               
                    }

                    break;
                case CommandType.CMD_NODEEVENT:
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

        public void StartTimer()
        {
            _times = _timeRepository.GetAll();

            _scheduler.Start();
        }

        public void StopTimer()
        {
            _scheduler.Stop();
        }

        public void ResetTimer()
        {
            _scheduler.Reset();
        }

        public NodeModel GetNodebyUniqueId(string uniqueId)
        {
            var node = _nodeService.GetNodeByUniqueId(uniqueId);
            return node ?? null;
        }

       
    }
}
