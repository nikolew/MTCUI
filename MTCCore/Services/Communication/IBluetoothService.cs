using System;
using System.Threading.Tasks;

namespace MTCCore.Services.Communication
{
    public interface IBluetoothService
    {
        event EventHandler<byte[]> PacketReceived;
        event EventHandler<bool> ConnectionStateChanged;

        Task StartDiscoveryAsync();
        Task SendAsync(byte[] data);
        void Disconnect();
    }
}
