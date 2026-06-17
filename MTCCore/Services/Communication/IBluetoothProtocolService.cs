using MTCCore.Protocol;
using System.Threading.Tasks;

namespace MTCCore.Services.Communication
{
    public interface IBluetoothProtocolService
    {
        void Start();
        void Stop();
 
        Task SendDataAsync(Envelope packet);
    }
}
