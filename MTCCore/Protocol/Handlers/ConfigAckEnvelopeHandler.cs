using MTCCore.Protocol.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MTCCore.Protocol.Handlers
{
    public class ConfigAckEnvelopeHandler : IEnvelopeHandler
    {
        public event EventHandler<ConfigAckEnvelopeEventArgs> ConfigAck;
        

        public void Handle(Envelope envelope)
        {
            ConfigAck?.Invoke(this, new ConfigAckEnvelopeEventArgs(envelope.ConfigAck));
        }
    }
}
