using MTCCore.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Messages.Master
{
    public record MasterCommandMessage(int Command);

    public record MasterConfigCommandMessage(Envelope packet);
}
