using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class NodeReadConfigHandler : IPacketHandler
    {
        public CommandType PayloadType => CommandType.CMD_NODEREADCONFIG;

        public event EventHandler<NodeConfigReceivedEventArgs> NodeConfigReceived;

        public void Handle(Packet packet)
        {
            NodeConfigReceived?.Invoke(this, new NodeConfigReceivedEventArgs(packet.NodeConfig));
        }
    }
}
