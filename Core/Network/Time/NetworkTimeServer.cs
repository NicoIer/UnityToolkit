// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MemoryPack;
using UnityToolkit;

namespace Network.Time
{
    public class NetworkTimeServer
    {
        private CancellationTokenSource _cts;
        public long timestampMs => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public NetworkTimeServer()
        {
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        public Task Start(ushort port)
        {
            _cts = new CancellationTokenSource();
            return Task.Run(async () =>
            {
                IPAddress ip = IPAddress.Any;
                IPEndPoint point = new IPEndPoint(ip, port);
                Socket udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpServer.Bind(point);
                ToolkitLog.Info($"NetworkTimeServer Start At {ip}:{port} {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                byte[] receiveBuffer = new byte[1024];
                NetworkBuffer sendBuffer = new NetworkBuffer();

                EndPoint clientPoint = new IPEndPoint(IPAddress.Any, 0);
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // var result = await udpServer.ReceiveFromAsync(receiveBuffer, SocketFlags.None, clientPoint);
                        int length = udpServer.ReceiveFrom(receiveBuffer, ref clientPoint);

                        sendBuffer.Reset();
                        ClientSyncTimeMessage msg =
                            MemoryPackSerializer.Deserialize<ClientSyncTimeMessage>(
                                new ArraySegment<byte>(receiveBuffer, 0, length));

                        ServerSyncTimeMessage serverSyncTimeMessage = ServerSyncTimeMessage.From(ref msg);
                        MemoryPackSerializer.Serialize(sendBuffer, serverSyncTimeMessage);
                        // 回复消息
                        // udpServer.Send(sendBuffer.buffer);
                        await udpServer.SendToAsync(sendBuffer.ToArraySegment(), SocketFlags.None, clientPoint);
                    }
                    catch (SocketException e)
                    {
                        ToolkitLog.Error(e);
                    }
                }
            }, cancellationToken: _cts.Token);
        }
    }
}