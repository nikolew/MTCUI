using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeStatusReceivedEventArgs
    {
        public NodeStatus NodeStatus { get; set; }
        public NodeStatusReceivedEventArgs(NodeStatus nodeStatus)
        {
            NodeStatus = nodeStatus;
        }
    }
}
