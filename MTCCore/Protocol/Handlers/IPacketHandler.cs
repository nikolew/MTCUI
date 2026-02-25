using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Enumeration;

namespace MTCCore.Protocol.Handlers
{
    public interface IPacketHandler
    {
        CommandType PayloadType { get; }
        void Handle(Packet packet);
    }
}
