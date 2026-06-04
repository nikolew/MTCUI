using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeListEnvelopeReceivedEventArgs
    {
        public NodeListResp NodeListResponse { get; set; }

        public NodeListEnvelopeReceivedEventArgs(NodeListResp nodeListResp)
        {
            NodeListResponse = nodeListResp;
        }
    }
}
