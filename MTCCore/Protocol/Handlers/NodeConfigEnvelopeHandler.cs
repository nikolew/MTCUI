using MTCCore.Protocol.Events;
using System;

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
