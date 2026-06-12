using MTCCore.Protocol.Events;
using System;

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
