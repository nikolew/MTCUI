using MTCCore.Protocol.Events;
using System;

namespace MTCCore.Protocol.Handlers
{
    public class NodeEventHandler : IPacketHandler
    {
        public CommandType PayloadType => CommandType.CMD_NODEEVENT;

        public event EventHandler<NodeEventReceivedEventErgs> NodeEventReceived;

        public void Handle(Packet packet)
        {
            NodeEventReceived?.Invoke(this, new NodeEventReceivedEventErgs(packet.NodeEvent));  
        }
    }
}
