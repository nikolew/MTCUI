using MTCCore.Protocol.Events;
using System;

namespace MTCCore.Protocol.Handlers
{
    public class PingHandler : IPacketHandler
    {
        public CommandType PayloadType => CommandType.CMD_PING;

        event EventHandler<PingReceivedEventArgs> PongReceived;

        public void Handle(Packet packet)
        {
            PongReceived?.Invoke(this, new PingReceivedEventArgs());
        }
    }
}
