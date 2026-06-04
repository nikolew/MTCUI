using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public class NodeConfigEnvelopeEventArgs
    {
        public ConfigNodeResp NodeConfig { get; set; }

        public NodeConfigEnvelopeEventArgs(ConfigNodeResp nodeConfig)
        {
            NodeConfig = nodeConfig;
        }
    }
}
