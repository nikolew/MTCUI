using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeEventReceivedEventErgs
    {
        public NodeEvent NodeEvent { get; set; }

        public NodeEventReceivedEventErgs(NodeEvent nodeEvent)
        {
            NodeEvent = nodeEvent;   
        }
    }
}
