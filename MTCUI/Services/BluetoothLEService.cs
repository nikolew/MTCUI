using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using MTCUI.Models;
using ProtoBuf;

namespace MTCUI.Services;

public class BluetoothLEService
{
    private const string TargetName = "MTC-01";

    private readonly Guid ServiceUuid = Guid.Parse("000000ff-0000-1000-8000-00805f9b34fb");
    private readonly Guid CharUuid    = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb");

    private BluetoothLEAdvertisementWatcher _watcher;
    private BluetoothLEDevice _device;
    private GattCharacteristic _characteristic;

    private ulong _lastAddress;
    private bool _isReconnecting;

    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _heartbeatTimeout  = TimeSpan.FromSeconds(20);
    private DateTime _lastHeartbeatResponse = DateTime.MinValue;
    private CancellationTokenSource _heartbeatCts;

    public event Action<string> StatusChanged;
    public event Action<byte[]> ResponseReceived;
    public event Action<bool> ConnectionChanged; // true = connected, false = disconnected

    public void StartDiscovery()
    {
        StatusChanged?.Invoke("Scanning for MTC-01...");

        _watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };
        _watcher.Received += OnAdvertisement;
        _watcher.Start();
    }

    private async void OnAdvertisement(BluetoothLEAdvertisementWatcher sender,
                                       BluetoothLEAdvertisementReceivedEventArgs args)
    {
        if (args.Advertisement.LocalName == TargetName)
        {
            _watcher.Stop();
            _watcher.Received -= OnAdvertisement;
            _lastAddress = args.BluetoothAddress;

            //StatusChanged?.Invoke("Found MTC-01");
            await ConnectAsync(_lastAddress);
        }
    }

    public async Task ConnectAsync(ulong address)
    {
        try
        {
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            if (_device == null)
            {
                StatusChanged?.Invoke("Failed to connect.");
                ConnectionChanged?.Invoke(false);
                return;
            }

            _device.ConnectionStatusChanged += OnConnectionStatusChanged;

            StatusChanged?.Invoke("Connected. Discovering services...");

            var servicesResult = await _device.GetGattServicesAsync();
            if (servicesResult.Status != GattCommunicationStatus.Success)
            {
                StatusChanged?.Invoke("Service discovery failed.");
                ConnectionChanged?.Invoke(false);
                return;
            }

            var service = servicesResult.Services.FirstOrDefault(s => s.Uuid == ServiceUuid);
            if (service == null)
            {
                StatusChanged?.Invoke("Service not found.");
                ConnectionChanged?.Invoke(false);
                return;
            }

            var charsResult = await service.GetCharacteristicsAsync();
            _characteristic = charsResult.Characteristics.FirstOrDefault(c => c.Uuid == CharUuid);

            if (_characteristic == null)
            {
                StatusChanged?.Invoke("Characteristic not found.");
                ConnectionChanged?.Invoke(false);
                return;
            }

            _characteristic.ValueChanged += OnValueChanged;

            await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify);

            StatusChanged?.Invoke("Ready.");
            ConnectionChanged?.Invoke(true);

            StartHeartbeatLoop();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke("Connect error: " + ex.Message);
            ConnectionChanged?.Invoke(false);
        }
    }

    private async void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            StatusChanged?.Invoke("Disconnected.");
            ConnectionChanged?.Invoke(false);
            StopHeartbeatLoop();

            if (_isReconnecting)
                return;

            _isReconnecting = true;
            await Task.Delay(2000);

            while (true)
            {
                try
                {
                    StatusChanged?.Invoke("Trying to reconnect...");
                    await ConnectAsync(_lastAddress);

                    if (_device != null &&
                        _device.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    {
                        StatusChanged?.Invoke("Reconnected.");
                        _isReconnecting = false;
                        return;
                    }
                }
                catch { }

                await Task.Delay(3000);
            }
        }
    }

    private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        byte[] data = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(data);

        _lastHeartbeatResponse = DateTime.Now;
        
        ResponseReceived?.Invoke(data);
    }

    public async Task SendPingAsync(uint id, string payload)
    {
        if (_characteristic == null)
            return;

        var req = new Packet() { CommandType = CommandType.CMD_PING };
        
        await Send(req);
    }

    public async Task Send(Packet packet)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, packet);
        var data = ms.ToArray();

        var writer = new DataWriter();
        writer.WriteBytes(data);
        var buffer = writer.DetachBuffer();

        if(_characteristic != null)
            await _characteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
    }

    private void StartHeartbeatLoop()
    {
        StopHeartbeatLoop(); // safety

        _heartbeatCts = new CancellationTokenSource();
        var token = _heartbeatCts.Token;

        _ = Task.Run(async () =>
        {
            _lastHeartbeatResponse = DateTime.Now;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await SendPingAsync(999, "heartbeat");
                }
                catch { }

                await Task.Delay(_heartbeatInterval, token);

                if (DateTime.Now - _lastHeartbeatResponse > _heartbeatTimeout)
                {
                    StatusChanged?.Invoke("Heartbeat timeout. Forcing reconnect...");
                    // ще задейства reconnect през ConnectionStatusChanged, ако disconnect-неш
                    try
                    {
                        _device?.Dispose();
                    }
                    catch { }
                    ConnectionChanged?.Invoke(false);
                    return;
                }
            }
        }, token);
    }

    private void StopHeartbeatLoop()
    {
        try
        {
            _heartbeatCts?.Cancel();
            _heartbeatCts?.Dispose();
        }
        catch { }
        _heartbeatCts = null;
    }


}
