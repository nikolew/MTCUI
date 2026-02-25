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

        private readonly Dictionary<CommandType, IPacketHandler> _handlers;

        public BluetoothProtocolService(IBluetoothService bluetoothService, IEnumerable<IPacketHandler> handlers)
        {
            _bluetooth = bluetoothService;

            _handlers = handlers.ToDictionary(h => h.PayloadType);

            _bluetooth.PacketReceived += OnPacketReceived;
        }

        public async Task SendDataAsync(Packet packet)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, packet);
            var data = ms.ToArray();

            await _bluetooth.SendAsync(data);
        }

        private async Task SendPingAsync()
        {
            var ping = new Packet() { CommandType = CommandType.CMD_PING };
            await SendDataAsync(ping);
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
            var packet = Serializer.Deserialize<Packet>(new MemoryStream(data));    
            Dispatch(packet);
        }

        private void Dispatch(Packet packet)
        {
            if (_handlers.TryGetValue(packet.CommandType, out var handler))
            {
                handler.Handle(packet);
            }
            else
            {
                // unknown packet
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
                    WeakReferenceMessenger.Default.Send(
                        new BluetoothStatusMessage("Protocol heartbeat timeout")
                    );

                    Stop();
                    return;
                }
            }
        }

    }
}
