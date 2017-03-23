﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocksCore.Primitives
{

    public interface IClientDataPolled
    {
        event EventHandler DataPolled;
    }

    public class TcpClientEx : TcpClient, ITlvClient, IDestinationEndPointHolder, IClientDataPolled
    {
        private const int BufferSize = 32 * 1024; // 32KB




        private byte[] ReadBuffer = new byte[BufferSize];


        private const uint KeepAliveTimeout = 10000;
        private const uint KeepAliveInterval = 10000;

        public event EventHandler Disconnected;
        public event EventHandler<PacketData> DataReceived;

        public TcpClientEx()
        {

        }
        public TcpClientEx(Socket attachTo)
        {
            AttachToSocket(attachTo);
        }
        public void AttachToSocket(Socket socketToAttach)
        {
            if (socketToAttach == null)
                throw new ArgumentNullException($"{nameof(socketToAttach)} is null");
            Client = socketToAttach;
            Client.SetupSocketTimeouts(new SocketSettings
            {
                NetworkClientKeepAliveInterval = KeepAliveInterval,
                NetworkClientKeepAliveTimeout = KeepAliveTimeout
            });
        }

        public async void BeginReceive()
        {

            while (Client != null && Client.Connected)
            {
                await BeginReceive(ReadBuffer);
            }

        }

        private async Task BeginReceive(byte[] readTo)
        {
            try
            {

                var stream = GetStream();

                var readedBytesCount = await stream.ReadAsync(readTo, 0, readTo.Length).ConfigureAwait(false);
                if (readedBytesCount >= 1)
                {
                    DataReceived?.Invoke(this, new PacketData
                    {
                        Buffer = readTo,
                        BytesCount = readedBytesCount
                    });
                }
                else
                {
                    DoOnDisconnected();
                }
            }
            catch (Exception)
            {
                Client.Close();
                DoOnDisconnected();
            }
        }


        private void DoOnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public byte[] PeekBytes(int bytesCount)
        {
            return Client.PeekBytes(bytesCount);
        }

        public byte[] Receive(int bytesCount)
        {
            return Client.ReadComplete(bytesCount);
        }

        public void Send(byte[] arrayToSend)
        {
            Client.Send(arrayToSend);
        }

        public IPEndPoint ConnectedToEndPoint => (IPEndPoint)Client.RemoteEndPoint;
        public event EventHandler DataPolled;
    }
}