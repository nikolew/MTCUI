using MTCCore.Protocol.Events;
using System;


namespace MTCCore.Protocol.Handlers
{
    public class NodeDataEnvelopeHandler : IEnvelopeHandler
    {
        public event EventHandler<NodeDataEnvelopeReceivedEventArgs> NodeDataEnvelopeReceived;

        public void Handle(Envelope envelope)
        {
            NodeDataEnvelopeReceived?.Invoke(this, new NodeDataEnvelopeReceivedEventArgs(envelope.NodeData));
        }
    }
}
