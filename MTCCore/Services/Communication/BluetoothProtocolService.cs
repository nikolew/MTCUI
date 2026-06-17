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
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, packet);
            var data = ms.ToArray();

            await _bluetooth.SendAsync(data);
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

        private void OnPacketReceived(object sender, byte[] data)
        {
            _lastPong = DateTime.Now;
            byte[] payload = new byte[data.Length - 2];
            Array.Copy(data, 2, payload, 0, payload.Length);

            var packet = Serializer.Deserialize<Envelope>(new MemoryStream(payload));    
            Dispatch(packet);
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
