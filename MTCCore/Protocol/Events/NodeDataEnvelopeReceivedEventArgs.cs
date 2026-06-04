using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeDataEnvelopeReceivedEventArgs
    {
        public NodeDataEvent NodeData { get; set; }

        public NodeDataEnvelopeReceivedEventArgs(NodeDataEvent nodeData)
        {
            NodeData = nodeData;
        }
    }
}
