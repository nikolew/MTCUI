using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class PingEnvelopeHandler : IEnvelopeHandler
    {
        event EventHandler<PingReceivedEnvelopeEventArgs> PongReceived;
        public void Handle(Envelope envelope)
        {
            PongReceived?.Invoke(this, new PingReceivedEnvelopeEventArgs());
        }
    }
}
