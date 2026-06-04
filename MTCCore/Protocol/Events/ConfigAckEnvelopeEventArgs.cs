using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Protocol.Events
{
    public  class ConfigAckEnvelopeEventArgs
    {
        public ConfigAckResp ConfigAck { get; set; }
        public ConfigAckEnvelopeEventArgs(ConfigAckResp configAck)
        {
            ConfigAck = configAck;
        }
    }
}
