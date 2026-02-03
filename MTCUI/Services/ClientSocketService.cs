using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MTCUI.Services
{
    public class ClientSocketService
    {
        public event Action<bool> ClientStateChanged;
        public event Action<byte[]> ClientMessageReceived;

        private Socket _socket;
        private SocketAsyncEventArgs _sendArgs;
        private SocketAsyncEventArgs _receiveArgs;
        private Queue<byte[]>? _sendQueue;
        private readonly AsyncOperation? _asyncOperation;
        private static readonly byte[] receiveBuffer = new byte[1024];

        public bool IsConnected { get; set; }

        private byte[] _buffer = new byte[4096];
        private int _expectedPayloadLength = -1;
        private int _receivedPayload = 0;
        private byte[] _payloadBuffer;


        public ClientSocketService()
        {
            _asyncOperation = AsyncOperationManager.CreateOperation(null);
            _sendArgs = new SocketAsyncEventArgs();
            _receiveArgs = new SocketAsyncEventArgs();

            _sendQueue = new Queue<byte[]>();
        }

        public void Connect(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port)
            };

            connectArgs.Completed += OnConnectCompleted;

            if (!_socket.ConnectAsync(connectArgs))
            {
                OnConnectCompleted(_socket, connectArgs);
            }
        }

        private void OnConnectCompleted(object clientSocket, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                IsConnected = true;
                _asyncOperation?.Post(x => OnClientState((bool)x), true);

                _receiveArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                _receiveArgs.Completed += OnReceiveCompleted;

                if (!_socket.ReceiveAsync(_receiveArgs))
                {
                    OnReceiveCompleted(clientSocket, _receiveArgs);
                }
            }
            else
            {
                Disconnect();
            }
        }


        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                ProcessReceive(e);
                if (!_socket.ReceiveAsync(_receiveArgs))
                {
                    OnReceiveCompleted(null, _receiveArgs);
                }
            }
            else
            {
                Disconnect();
            }

        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                Console.WriteLine("Disconnected or error.");
                return;
            }

            var offset = 0;

            while (offset < e.BytesTransferred)
            {
                if (_expectedPayloadLength == -1)
                {
                    if (e.BytesTransferred - offset < 4)
                    {
                        Console.WriteLine("Incomplete header received.");
                        return;
                    }

                    _expectedPayloadLength = (e.Buffer[offset] << 24) |
                                             (e.Buffer[offset + 1] << 16) |
                                             (e.Buffer[offset + 2] << 8) |
                                             (e.Buffer[offset + 3]);

                    _payloadBuffer = new byte[_expectedPayloadLength];
                    _receivedPayload = 0;
                    offset += 4;

                    Console.WriteLine($"Expecting {_expectedPayloadLength} bytes of payload.");
                }

                var bytesToCopy = Math.Min(_expectedPayloadLength - _receivedPayload, e.BytesTransferred - offset);
                Array.Copy(e.Buffer, offset, _payloadBuffer, _receivedPayload, bytesToCopy);
                _receivedPayload += bytesToCopy;
                offset += bytesToCopy;

                if (_receivedPayload != _expectedPayloadLength)
                    continue;
                ClientMessageReceived?.Invoke(_payloadBuffer);

                _expectedPayloadLength = -1;
                _receivedPayload = 0;
                _payloadBuffer = null;
            }
        }

        private void Disconnect()
        {
            IsConnected = false;

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            _asyncOperation?.Post(x => OnClientState(false), null);
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {

            }
        }

        private void OnClientState(bool connected)
        {
            ClientStateChanged?.Invoke(connected);
        }

        public void Send(byte[] data)
        {
            if (!IsConnected)
                return;

            _sendQueue?.Enqueue(data);
            HandleSend();
        }

        private void HandleSend()
        {
            var data = _sendQueue?.Dequeue();
            _sendArgs.SetBuffer(new byte[data.Length], 0, data.Length);
            _sendArgs.Completed += OnSendCompleted;

            // Prepare header (4 bytes, big-endian)
            var length = data.Length;
            var header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));

            // Combine header + payload
            var packet = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(data, 0, packet, header.Length, data.Length);

            _sendArgs.SetBuffer(new byte[packet.Length], 0, packet.Length);

            if (_sendArgs.Buffer != null) Array.Copy(packet, 0, _sendArgs.Buffer, 0, packet.Length);

            if (!_socket.SendAsync(_sendArgs))
            {
                OnSendCompleted(_socket, _sendArgs);
            }
        }
    }
}
