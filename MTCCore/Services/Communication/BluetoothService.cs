using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Messages.Bluetooth;
using System;
using System.Linq;
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

        private readonly Guid ServiceUuid = Guid.Parse("000000ff-0000-1000-8000-00805f9b34fb");
        private readonly Guid CharUuid = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb");

        private BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEDevice _device;
        private GattCharacteristic _characteristic;

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

                var service = servicesResult.Services
                    .FirstOrDefault(s => s.Uuid == ServiceUuid);
                if (service == null)
                    return;

                var charsResult = await service.GetCharacteristicsAsync();
                _characteristic = charsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == CharUuid);

                if (_characteristic == null)
                    return;

                _characteristic.ValueChanged += OnValueChanged;

                await _characteristic
                    .WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

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
        private void OnValueChanged(
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
            if (_characteristic == null)
                return;

            var writer = new DataWriter();
            writer.WriteBytes(data);

            await _characteristic.WriteValueAsync(
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
                _characteristic.ValueChanged -= OnValueChanged;
                _device.ConnectionStatusChanged -= OnConnectionStatusChanged;

                _device?.Dispose();
            }
            catch { }

            _device = null;
            _characteristic = null;

            ConnectionStateChanged?.Invoke(this, false);
        }
    }
}
