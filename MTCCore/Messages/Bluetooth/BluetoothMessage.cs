using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Messages.Bluetooth
{
    public record BluetoothConnectMessage();
    public record BluetoothDisconnectMessage();
    public record BluetoothStatusMessage(string Status);
    public record BluetoothResponseMessage(byte[] Response);

}
