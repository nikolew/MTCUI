using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Handlers
{
    public class NodeStatusEnvelopeHandler : IEnvelopeHandler
    {
        public event EventHandler<NodeStatusEnvelopeEventArgs> NodeStatus;

        public void Handle(Envelope envelope)
        {
            NodeStatus?.Invoke(this, new NodeStatusEnvelopeEventArgs(envelope.NodeStatus));
        }
    }
}
