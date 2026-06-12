using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeStatusEnvelopeEventArgs
    {
        public NodeStatusEvent NodeStatus { get; set; }

        public NodeStatusEnvelopeEventArgs(NodeStatusEvent nodeStatus    )
        {
            NodeStatus = nodeStatus;
        }
    }
}
