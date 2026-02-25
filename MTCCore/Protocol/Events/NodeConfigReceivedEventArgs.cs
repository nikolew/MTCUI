using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeConfigReceivedEventArgs
    {
        public NodeConfig NodeConfig { get; set; }
        public NodeConfigReceivedEventArgs(NodeConfig nodeConfig)
        {
            NodeConfig = nodeConfig;
        }
    }
}
