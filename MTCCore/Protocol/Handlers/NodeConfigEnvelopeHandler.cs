using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class NodeConfigEnvelopeHandler : IEnvelopeHandler
    {
        public event EventHandler<NodeConfigEnvelopeEventArgs> NodeConfigEnvelopeReceived;
        public void Handle(Envelope envelope)
        {
            NodeConfigEnvelopeReceived.Invoke(this, new NodeConfigEnvelopeEventArgs(envelope.ConfigNode));
        }
    }
}
