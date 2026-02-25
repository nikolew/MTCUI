using MTCCore.Protocol.Events;
using System;

namespace MTCCore.Protocol.Handlers
{
    public class NodeListHandler : IPacketHandler
    {
        public CommandType PayloadType => CommandType.CMD_GETNODES;

        public event EventHandler<NodeListReceivedEventArgs> NodeListReceived;


        public void Handle(Packet packet)
        {
            NodeListReceived?.Invoke(this, new NodeListReceivedEventArgs(packet.NodeList));
        }
    }
}
