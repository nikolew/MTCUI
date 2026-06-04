using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using MTCCore.Data;
using MTCCore.Domain.Entities;
using MTCCore.Domain.Enums;
using MTCCore.DTO.Nodes;
using MTCCore.Extensions.Nodes;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using MTCCore.Protocol.Events;
using MTCCore.Protocol.Handlers;
using MTCCore.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace MTCCore.Services.Nodes
{
    public class NodeService : INodeService
    {
        private ApplicationDbContext _dbContext;
        private readonly IBluetoothProtocolService _bluetoothProtocol;
        private readonly NodeListEnvelopeHandler _nodeListEnvelope;
        private readonly NodeDataEnvelopeHandler _nodeData;

        public NodeService(ApplicationDbContext dbContext, 
            IBluetoothProtocolService bluetoothProtocol,
            NodeListEnvelopeHandler nodeListEnvelope,
            NodeDataEnvelopeHandler nodeData)
        {
            _dbContext = dbContext;
            _bluetoothProtocol = bluetoothProtocol;
            _nodeListEnvelope = nodeListEnvelope;
            _nodeData = nodeData;

            _nodeListEnvelope.NodeListReceived += Envelope_NodeListReceived;
            _nodeData.NodeDataEnvelopeReceived += Envelope_NodeDataReceived;

            //WeakReferenceMessenger.Default.Register<MasterCommandMessage>(this, (r, m) => { OnMasterCommand(m.Command); });
            //WeakReferenceMessenger.Default.Register<NodeSendCommandMessage>(this, (r, m) => { OnNodeCommand(m.Id, m.CommandType); });
            //WeakReferenceMessenger.Default.Register<NodeSetConfigMessage>(this, (r, m) => { SendNodeConfiguration(m.NodeConfig); });
            //WeakReferenceMessenger.Default.Register<NodeSaveMessage>(this, (r, m) => { SaveScene(m.Nodes); });

            //WeakReferenceMessenger.Default.Register<SendNodeCommand>(this, (r, m) =>
            //{
            //    _bluetoothProtocol.SendDataAsync(m.envelope);
            //});
        }

        


        // ==========================================================================================

        private void Envelope_NodeListReceived(object sender, NodeListEnvelopeReceivedEventArgs e)
        {
            if (e.NodeListResponse.Nodes is null)
                return;

            bool saveFlag = false;

            var updatedNodes = new List<ReadNodeDto>();
            var nodes = _dbContext.Nodes
                    .Include(a => a.Position)
                    .Include(b => b.GroupEnttity).ToList();

            foreach (var protoNode in e.NodeListResponse.Nodes)
            {
                var uniqueId = Convert.ToHexString(protoNode.Uid);
                var node = nodes.Where(x => x.NodeUniqueId == uniqueId).FirstOrDefault();

                if(node is null)
                {
                    var positionEntity = new PositionEntity { X = 50, Y = 50 };
                    var groupEntity = _dbContext.Groups.FirstOrDefault(x => x.GroupName == "None");

                    node = new NodeEntity
                    {
                        GroupEnttityId = 1,
                        NodeUniqueId = uniqueId,
                        NodeIdentity = protoNode.NodeId,
                        Distance = "",
                        Position = positionEntity,
                        TargetType = TargetType.Default,
                        GroupEnttity = groupEntity
                    };

                    _dbContext.Nodes.Add(node);
                    saveFlag = true;

                    updatedNodes.Add(new ReadNodeDto
                    {
                        UniqueNodeId = uniqueId,
                        TargetType = node.TargetType,
                        GroupName = node.GroupEnttity.GroupName,
                        Distance = node.Distance,
                        NodeId = node.NodeIdentity,
                        State = TargetState.TargetRaised,
                        Position = new Point(node.Position.X, node.Position.Y)
                    });
                }
                else
                {
                    var grEntity = _dbContext.Groups.FirstOrDefault(x => x.Id == node.GroupEnttityId);
                    updatedNodes.Add(new ReadNodeDto
                    {
                        UniqueNodeId = uniqueId,
                        TargetType = node.TargetType,
                        GroupName = grEntity.GroupName,
                        Distance = node.Distance,
                        NodeId = node.NodeIdentity,
                        State = TargetState.TargetRaised,
                        Position = new Point(node.Position.X, node.Position.Y)
                    });
                }
            }

            if (saveFlag)
                _dbContext.SaveChanges();

            // 📤 notify UI
            WeakReferenceMessenger.Default.Send(new NodeListRequestMessage(updatedNodes));

        }

        private void Envelope_NodeDataReceived(object sender, NodeDataEnvelopeReceivedEventArgs e)
        {
            var data = e.NodeData;

            if (data != null) 
            {
                var position = e.NodeData.Reading.Position;
                var state = e.NodeData.Reading.State;
                var targetState = TargetState.TargetFolded;

                if (position == 0 && state == 0)
                {
                    targetState = TargetState.TargetRaised;
                }
                else if (position == 1 && state == 0)
                {
                    targetState = TargetState.TargetFolded;
                }
                else if (state == 1)
                {
                    targetState = TargetState.TargetHit;
                }


                WeakReferenceMessenger.Default.Send(new NodeUpdateStatusMessage(new NodeModel
                {
                    NodeId = e.NodeData.NodeId,
                    State = targetState,
                    Rssi = e.NodeData.Rssi,
                    Snr = e.NodeData.Snr,
                    BattVoltage = e.NodeData.Reading.VoltageMv
                }));
            }

        }


        public void LoadScene()
        {
            var nodes = new GetNodeListReq() { ActiveOnly = true };
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                GetNodeList = nodes
            };

            _bluetoothProtocol.SendDataAsync(packet);
        }

        public async Task SaveScene(List<SaveNodeDto> nodes)
        {
            if (nodes.Count == 0)
                return;

            foreach (var node in nodes)
            {
                var nodeEntity = _dbContext.Nodes
                    .Include(p => p.Position)
                    .SingleOrDefault(x => x.NodeIdentity == node.NodeId);

                if (nodeEntity == null)
                    return;

                nodeEntity.Position.X = (int)node.Position.X;
                nodeEntity.Position.Y = (int)node.Position.Y;

                await _dbContext.SaveChangesAsync();
            }
        }


        public void NodeCommand(Envelope packet)
        {
            _bluetoothProtocol.SendDataAsync(packet);
        }

        //===========================================================================================




        private void OnNodeEventReceived(object sender, NodeEventReceivedEventErgs e)
        {
            var nodeEventModel = new NodeEventModel
            {
                Id = e.NodeEvent.Id,
                Online = e.NodeEvent.Online,
                MissedFrames = e.NodeEvent.MissedFrames,
                LastSeenMs = e.NodeEvent.LastSeenMs
            };

            WeakReferenceMessenger.Default.Send(new NodeEventMessage(nodeEventModel));
        }

        private async void SendNodeConfiguration(NodeConfigModel nodeConfig)
        {
            var packet = new Packet
            {
                CommandType = CommandType.CMD_NODESETCONFIG,
                NodeConfig = new NodeConfig
                {
                    Id = nodeConfig.NodeId,
                    LightMode = (int)nodeConfig.Light,
                    GroupId = nodeConfig.GroupId
                }
            };
            await Send(packet);
        }

        private void OnNodeConfigReceived(object sender, NodeConfigReceivedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new NodeGetConfigMessage(new NodeConfigModel
            {
                NodeId = e.NodeConfig.Id,
                Light = (LightMode)e.NodeConfig.LightMode,
                GroupId = e.NodeConfig.GroupId
            }));
        }

        // Handler for receiving node status updates from the Bluetooth protocol
        private void OnNodeStatusReceived(object sender, NodeStatusReceivedEventArgs e)
        {
            var status = e.NodeStatus;
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
                NodeId = status.Id,
                State = state,
                Rssi = status.Rssi,
                Snr = status.Snr,
                BattVoltage = voltage
            }));
        }

        // Handler for receiving the list of nodes from the Bluetooth protocol
        private void OnNodeListReceived(object sender, NodeListReceivedEventArgs e)
        {
            bool saveFlag = false;  

            var updatedNodes = new List<ReadNodeDto>();

            var nodes = _dbContext.Nodes
                    .Include(a => a.Position)
                    .Include(b => b.GroupEnttity).ToList();
                    

            foreach (var protoNode in e.NodeList.Nodes)
            {
                var uniqueId = Convert.ToHexString(protoNode.UniqueId);
                var node = nodes.FirstOrDefault(x => x.NodeUniqueId == uniqueId);

                if (node == null)
                {
                    var positionEntity = new PositionEntity { X = 50, Y = 50 };
                    var groupEntity = _dbContext.Groups.FirstOrDefault(x => x.GroupName == "None");
                    
                    node = new NodeEntity
                    {
                        GroupEnttityId = 1,
                        NodeUniqueId = uniqueId,
                        NodeIdentity = protoNode.NodeId,
                        Distance = "0",
                        Position = positionEntity,
                        TargetType = TargetType.Default,
                        GroupEnttity = groupEntity
                    };

                    _dbContext.Nodes.Add(node);
                    saveFlag = true;

                    updatedNodes.Add(new ReadNodeDto
                    {
                        UniqueNodeId = uniqueId,
                        TargetType = node.TargetType,
                        GroupName = node.GroupEnttity.GroupName,
                        Distance = node.Distance,
                        NodeId = node.NodeIdentity,
                        State = TargetState.TargetRaised,
                        Position = new Point(node.Position.X, node.Position.Y)
                    });
                }
                else
                {
                    var grEntity = _dbContext.Groups.FirstOrDefault(x => x.Id == node.GroupEnttityId);
                    updatedNodes.Add(new ReadNodeDto
                    {
                        UniqueNodeId = uniqueId,
                        TargetType = node.TargetType,
                        GroupName = grEntity.GroupName,
                        Distance = node.Distance,
                        NodeId = node.NodeIdentity,
                        State = TargetState.TargetRaised,
                        Position = new Point(node.Position.X, node.Position.Y)
                    });
                }
            }

            if (saveFlag) 
                _dbContext.SaveChanges();

            // 📤 notify UI
            WeakReferenceMessenger.Default.Send(new NodeListRequestMessage(updatedNodes));

        }
       

        // Method to add a new node to a group
        public async Task AddNodeAsync(int groupId, NodeModel node)
        {
            var groupExists = _dbContext.Groups.Where(g => g.Id == groupId).SingleOrDefault();

            if (groupExists is null)
                throw new InvalidOperationException("Group not found.");

            var positionEntity = new PositionEntity
            {
                X = (int)node.Position.X,
                Y = (int)node.Position.Y
            };

            var newnode = new NodeEntity
            {
                GroupEnttityId = groupId,
                GroupEnttity = groupExists,
                NodeUniqueId = node.UniqueNodeId,
                NodeIdentity = node.NodeId,
                Distance = "0",
                Position = positionEntity,
                TargetType = TargetType.Default
            };

            _dbContext.Nodes.Add(newnode);
            await _dbContext.SaveChangesAsync();
            return;
        }

        // Handle master commands from the UI
        private async void OnMasterCommand(int command)
        {
            var cmd = new Packet{ CommandType = CommandType.CMD_GETNODES };
            await Send(cmd);
        }

        // Handle node-specific commands from the UI
        private async void OnNodeCommand(int id, CommandType commandType)
        {
            var packet = new Packet { CommandType = commandType, Node = new Node { NodeId = id} };
            await Send(packet);
        }

        // Helper method to send a packet via Bluetooth
        private async Task Send(Packet packet)
        {
            await _bluetoothProtocol.SendDataAsync(packet);
        }

        // 
        public async Task UpdateNodesAsync(List<SaveNodeDto> nodes)
        {
            foreach (var node in nodes) 
            {

                var nodeEntity = _dbContext.Nodes
                    .Include(p => p.Position)
                    .SingleOrDefault(x => x.NodeIdentity == node.NodeId);

                if (nodeEntity == null)
                    return;

                nodeEntity.Position.X = (int)node.Position.X;
                nodeEntity.Position.Y = (int)node.Position.Y;

                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task UpdateNodeAsync(SaveNodeDto dto)
        {
            var nodeEntity = _dbContext.Nodes
                .Include(p => p.Position)
                .Include(g =>g.GroupEnttity)
                .SingleOrDefault(x => x.NodeIdentity == dto.NodeId);

            if(nodeEntity == null) 
                return;

            nodeEntity.TargetType = dto.TargetType;
            nodeEntity.GroupEnttityId = dto.GroupId;
            nodeEntity.Distance = dto.Distance;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ReadNodeDto>> GetAllAsync()
        {
            var nodes = _dbContext.Nodes
                .Include(a => a.Position)
                .Include(g => g.GroupEnttity)
                .ToListAsync().Result;

            return nodes.Select(n => n.ToReadDto()).ToList();
        }


        



        public async Task<ReadNodeDto> GetNodeByUniqueIdAsync(int uniqueId)
        {
            var node = _dbContext.Nodes
                .Include(a => a.Position)
                .SingleOrDefaultAsync(x => x.NodeIdentity == uniqueId).Result;

            if (node == null)
                return null;

            var newNode = new ReadNodeDto
            {
                UniqueNodeId = node.NodeUniqueId,
                NodeId = node.NodeIdentity,
                Position = new Point(node.Position.X, node.Position.Y),
                TargetType = node.TargetType,
                Distance = node.Distance
            };

            return newNode;
        }

        public bool NodeExists(string uniqueId)
        {
            var node = _dbContext.Nodes
                .Include(a => a.Position)
                .SingleOrDefaultAsync(x => x.NodeUniqueId == uniqueId);
            return node != null;
        }

        

        public List<NodeModel> GetAllNodes()
        {
            var nodes = _dbContext.Nodes;

            return nodes.Select(node => new NodeModel
            {
                UniqueNodeId = node.NodeUniqueId,
                NodeId = node.NodeIdentity,
                Position = new Point(node.Position.X, node.Position.Y),
                TargetType = node.TargetType,
                State = TargetState.TargetOffline,
                Distance = node.Distance,
                GroupName = node.GroupEnttity.GroupName
            }).ToList();
        }

        public async Task UpdateNodes(IEnumerable<NodeModel> nodes)
        {
            foreach (var node in nodes)
            {
                var nodeEntity = _dbContext.Nodes
                    .Include(a => a.Position)
                    .SingleOrDefaultAsync(x => x.NodeUniqueId == node.UniqueNodeId).Result;

                if (nodeEntity == null)
                    continue;

                nodeEntity.Position.X = (int)node.Position.X;
                nodeEntity.Position.Y = (int)node.Position.Y;
                nodeEntity.TargetType = node.TargetType;
                nodeEntity.Distance = node.Distance;
                //nodeEntity.TargetGroup = node.Group;

                _dbContext.Update(nodeEntity);
                await _dbContext.SaveChangesAsync();
            }

        }

        //public async Task UpdateNode(NodeModel node)
        //{
        //    var nodeEntity = _dbContext.Nodes
        //        .Include(a => a.Position)
        //        .SingleOrDefaultAsync(x => x.NodeUniqueId == node.UniqueNodeId).Result;

        //    if (nodeEntity == null)
        //        return;

            
        //    nodeEntity.Position.X = (int)node.Position.X;
        //    nodeEntity.Position.Y = (int)node.Position.Y;
        //    nodeEntity.TargetType = node.TargetType;
        //    nodeEntity.Distance = node.Distance;
        //    nodeEntity.GroupEnttityId = node.GroupId;

        //    _dbContext.Update(nodeEntity);
        //    await _dbContext.SaveChangesAsync();
        //}

        public async Task CreateNodeAsync(CreateNodeDto dto)
        {
            var positionEntity = new PositionEntity
            {
                X = (int)dto.Position.X,
                Y = (int)dto.Position.Y
            };

            var newnode = new NodeEntity
            {
                GroupEnttityId = dto.GroupId,
                NodeUniqueId = dto.UniqueNodeId,
                NodeIdentity = dto.NodeId,
                Distance = dto.Distance,
                Position = positionEntity,
                TargetType = dto.TargetType
            };

            _dbContext.Nodes.Add(newnode);
            await _dbContext.SaveChangesAsync();
        }

        
    }
}
