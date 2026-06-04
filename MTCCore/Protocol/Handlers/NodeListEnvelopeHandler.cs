using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class NodeListEnvelopeHandler : IEnvelopeHandler
    {
        public event EventHandler<NodeListEnvelopeReceivedEventArgs> NodeListReceived;

        public void Handle(Envelope envelope)
        {
            NodeListReceived?.Invoke(this, new NodeListEnvelopeReceivedEventArgs(envelope.NodeList));
        }
    }
}
