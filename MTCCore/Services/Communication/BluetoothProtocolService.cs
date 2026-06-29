using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Protocol;
using MTCCore.Protocol.Handlers;
using MTCCore.Messages.Bluetooth;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTCCore.Services.Communication
{
    public class BluetoothProtocolService : IBluetoothProtocolService
    {
        private readonly IBluetoothService _bluetooth;

        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(20);

        private DateTime _lastPong = DateTime.MinValue;
        private CancellationTokenSource _cts;

        private readonly Dictionary<Type, IEnvelopeHandler> _envelopeHandlers;

        public BluetoothProtocolService(IBluetoothService bluetoothService, IEnumerable<IEnvelopeHandler> ehandlers)
        {
            _bluetooth = bluetoothService;

            _envelopeHandlers = ehandlers.ToDictionary(h => h.GetType());

            _bluetooth.PacketReceived += OnPacketReceived;
        }

        public async Task SendDataAsync(Envelope packet)
        {
            try
            {
                using var ms = new MemoryStream();
                Serializer.Serialize(ms, packet);
                var data = ms.ToArray();

                await _bluetooth.SendAsync(data);
            }
            catch (Exception ex)             
            { 
            }
        }

        private async Task SendPingAsync()
        {
            var pingReq = new PingReq { Payload = 12345 };
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                Ping = pingReq
            };

            await SendDataAsync(packet);
        }

        public void Start()
        {
            Stop();

            _lastPong = DateTime.Now;
            _cts = new CancellationTokenSource();

            _ = HeartbeatLoop(_cts.Token);
        }

        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch { }

            _cts = null;
        }

        private readonly List<byte> _rxBuffer = new List<byte>();

        private void OnPacketReceived(object sender, byte[] data)
        {
            //_lastPong = DateTime.Now;
            //byte[] payload = new byte[data.Length - 2];
            //Array.Copy(data, 2, payload, 0, payload.Length);

            //var packet = Serializer.Deserialize<Envelope>(new MemoryStream(payload));    
            //Dispatch(packet);

            _lastPong = DateTime.Now;
            _rxBuffer.AddRange(data);

            // обработи всички цели съобщения в буфера
            while (_rxBuffer.Count >= 2)
            {
                // length header = total size (включва 2-та байта, както master ги слага)
                int total = _rxBuffer[0] | (_rxBuffer[1] << 8);

                if (total < 2 || total > 4096)   // sanity
                {
                    _rxBuffer.Clear();           // повреден поток — ресет
                    break;
                }
                if (_rxBuffer.Count < total)
                    break;                        // още не е дошло цялото съобщение

                // извади payload-а (без 2-та length байта)
                byte[] payload = new byte[total - 2];
                _rxBuffer.CopyTo(2, payload, 0, total - 2);
                _rxBuffer.RemoveRange(0, total);

                try
                {
                    var packet = Serializer.Deserialize<Envelope>(new MemoryStream(payload));
                    Dispatch(packet);
                }
                catch (Exception ex)
                {
                    // лог; не чисти целия буфер — само това съобщение е лошо
                    Console.WriteLine($"Deserialize failed: {ex.Message}");
                }
            }
        }

        private void Dispatch(Envelope packet)
        {
            var type = packet.Pong != null ? typeof(PingEnvelopeHandler) :
                       packet.NodeList != null ? typeof(NodeListEnvelopeHandler) :
                       packet.NodeData != null ? typeof(NodeDataEnvelopeHandler) :
                       packet.ConfigAck != null ? typeof(ConfigAckEnvelopeHandler):
                       packet.ConfigNode != null ? typeof(NodeConfigEnvelopeHandler):
                       packet.NodeStatus != null ? typeof(NodeStatusEnvelopeHandler) :
                       null;

            if (_envelopeHandlers.TryGetValue(type, out var handler))
            {
                handler.Handle(packet);
            }
        }

        // ===============================
        // Heartbeat
        // ===============================
        private async Task HeartbeatLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await SendPingAsync();

                await Task.Delay(_heartbeatInterval, token);

                if (DateTime.Now - _lastPong > _heartbeatTimeout)
                {
                    WeakReferenceMessenger.Default.Send(new BluetoothStatusMessage("Protocol heartbeat timeout"));

                    Stop();
                    return;
                }
            }
        }

    }
}
