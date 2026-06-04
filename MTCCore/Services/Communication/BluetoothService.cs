using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Messages.Bluetooth;
using System;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace MTCCore.Services.Communication
{
    public class BluetoothService : IBluetoothService
    {
        public event EventHandler<byte[]> PacketReceived;
        public event EventHandler<bool> ConnectionStateChanged;

        private const string TargetName = "MTC-01";

        //private readonly Guid ServiceUuid = Guid.Parse("000000ff-0000-1000-8000-00805f9b34fb");
        //private readonly Guid CharUuid = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb");

        private readonly Guid ServiceUuid = BluetoothUuidHelper.FromShortId(0x1820);
        private static readonly Guid WriteCharGuid = BluetoothUuidHelper.FromShortId(0x2A06);
        private static readonly Guid NotifyCharGuid = BluetoothUuidHelper.FromShortId(0x2A07);

        private BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEDevice _device;
        private GattCharacteristic _characteristic;
        private GattCharacteristic? _writeChar;
        private GattCharacteristic? _notifyChar;

        private ulong _lastAddress;
        private bool _isReconnecting;

        // ===============================
        // Discovery
        // ===============================
        public async Task StartDiscoveryAsync()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _watcher.Received += OnAdvertisement;
            _watcher.Start();
        }

        private async void OnAdvertisement(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (args.Advertisement.LocalName != TargetName)
                return;

            _watcher.Stop();
            _watcher.Received -= OnAdvertisement;

            _lastAddress = args.BluetoothAddress;
            await ConnectAsync(_lastAddress);
        }

        // ===============================
        // Connection
        // ===============================
        private async Task ConnectAsync(ulong address)
        {
            try
            {
                _device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                if (_device == null)
                    return;

                _device.ConnectionStatusChanged += OnConnectionStatusChanged;

                var servicesResult = await _device.GetGattServicesAsync();
                if (servicesResult.Status != GattCommunicationStatus.Success)
                    return;

                var service = servicesResult.Services.FirstOrDefault(s => s.Uuid == ServiceUuid);
                if (service == null)
                    return;

                // Write характеристика
                var writeResult = await service.GetCharacteristicsForUuidAsync(WriteCharGuid, BluetoothCacheMode.Uncached);

                if (writeResult.Status != GattCommunicationStatus.Success ||
                    writeResult.Characteristics.Count == 0)
                    throw new InvalidOperationException("Write characteristic not found.");
                _writeChar = writeResult.Characteristics[0];

                // Notify характеристика
                var notifyResult = await service.GetCharacteristicsForUuidAsync(
                    NotifyCharGuid, BluetoothCacheMode.Uncached);
                if (notifyResult.Status != GattCommunicationStatus.Success ||
                    notifyResult.Characteristics.Count == 0)
                    throw new InvalidOperationException("Notify characteristic not found.");
                _notifyChar = notifyResult.Characteristics[0];

                // Абонираме се за notify
                _notifyChar.ValueChanged += OnNotifyValueChanged;
                var cccdStatus = await _notifyChar.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

                if (cccdStatus != GattCommunicationStatus.Success)
                    throw new InvalidOperationException(
                        $"Cannot enable notify: {cccdStatus}");

                WeakReferenceMessenger.Default.Send(new BluetoothStatusMessage("Ready."));
                ConnectionStateChanged?.Invoke(this, true);
            }
            catch
            {
                // swallow – reconnect loop ще се погрижи
            }
        }

        private async void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                ConnectionStateChanged?.Invoke(this, true);
                return;
            }

            ConnectionStateChanged?.Invoke(this, false);

            if (_isReconnecting)
                return;

            _isReconnecting = true;

            while (true)
            {
                try
                {
                    WeakReferenceMessenger.Default.Send(new BluetoothStatusMessage("Trying to reconnect..."));
                    await Task.Delay(3000);
                    await ConnectAsync(_lastAddress);

                    if (_device?.ConnectionStatus ==
                        BluetoothConnectionStatus.Connected)
                    {
                        WeakReferenceMessenger.Default.Send(new BluetoothStatusMessage("Reconnected."));
                        _isReconnecting = false;
                        return;
                    }
                }
                catch { }
            }
        }

        // ===============================
        // Receiving
        // ===============================
        private void OnNotifyValueChanged(
            GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);

            PacketReceived?.Invoke(this, data);
        }

        // ===============================
        // Sending
        // ===============================
        public async Task SendAsync(byte[] data)
        {
            if (_writeChar == null)
                return;

            var writer = new DataWriter();
            writer.WriteBytes(data);

            await _writeChar.WriteValueAsync(
                writer.DetachBuffer(),
                GattWriteOption.WriteWithoutResponse);
        }

        // ===============================
        // Cleanup
        // ===============================
        public void Disconnect()
        {
            try
            {
                _writeChar.ValueChanged -= OnNotifyValueChanged;
                _device.ConnectionStatusChanged -= OnConnectionStatusChanged;

                _device?.Dispose();
            }
            catch { }

            _device = null;
            _writeChar = null;

            ConnectionStateChanged?.Invoke(this, false);
        }
    }
}
