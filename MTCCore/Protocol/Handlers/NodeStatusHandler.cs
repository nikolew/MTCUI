using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class NodeStatusHandler : IPacketHandler
    {
        public CommandType PayloadType => CommandType.CMD_STATUS;
        public event EventHandler<NodeStatusReceivedEventArgs> NodeStatusReceived;

        public void Handle(Packet packet)
        {
            //NodeStatusReceived.Invoke(this, new NodeStatusReceivedEventArgs(packet.NodeStatus));
        }
    }
}
