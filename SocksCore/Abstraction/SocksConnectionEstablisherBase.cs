﻿using SocksCore.Primitives;
using System.Net;
using System.Net.Sockets;

namespace SocksCore
{
    public abstract class SocksConnectionEstablisherBase : ISocksConnectionEstablisher
    {
        public TcpClientEx ConnectTo(IPEndPoint connectTo)
        {
            var client = new TcpClientEx();

            try
            {
                client.Connect(connectTo);
            }
            catch (SocketException)
            {
                throw new ConnectionEstablisherException();
            }

            return client;
        }
    }
}