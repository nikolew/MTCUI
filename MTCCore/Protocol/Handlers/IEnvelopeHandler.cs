using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MTCCore.Protocol.Handlers
{
    public interface IEnvelopeHandler
    {
        void Handle(Envelope envelope);
    }
}
