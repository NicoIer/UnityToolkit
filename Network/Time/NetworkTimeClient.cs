using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MemoryPack;
using Network.Client;
using Network.Time;
using UnityToolkit;

namespace Network.Time
{
    public sealed class NetworkTimeClient
    {
        private CancellationTokenSource _cts;
        public long serverMs { get; private set; }
        public double rttMs => _rttEma.Value;

        private ExponentialMovingAverage _rttEma;
        public const int EmaSize = 4;

        public NetworkTimeClient()
        {
            _rttEma = new ExponentialMovingAverage(EmaSize);
        }


        public Task Run(string ip, int port, float intervalSeconds = 1.0f)
        {
            _cts = new CancellationTokenSource();

            IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpClient.Connect(point);

            Task sendTask = SendTask(udpClient, intervalSeconds);
            Task receiveTask = ReceiveTask(udpClient);

            return Task.WhenAll(sendTask, receiveTask);
        }

        private async Task ReceiveTask(Socket udpClient)
        {
            byte[] receiveBuffer = new byte[1024];

            while (!_cts.Token.IsCancellationRequested)
            {
                int length = udpClient.Receive(receiveBuffer);

                ServerSyncTimeMessage serverSyncTimeMessage =
                    MemoryPackSerializer.Deserialize<ServerSyncTimeMessage>(
                        new ArraySegment<byte>(receiveBuffer, 0, length));

                long clientSendMs = serverSyncTimeMessage.clientSendMs;
                long serverReceiveMs = serverSyncTimeMessage.serverReceiveMs;
                long nowMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                _rttEma.Add(nowMs - clientSendMs);

                serverMs = (long)(serverReceiveMs + rttMs / 2);
                await Task.Yield();
            }
        }

        private async Task SendTask(Socket udpClient, float intervalSeconds)
        {
            TimeSpan interval = TimeSpan.FromSeconds(intervalSeconds);
            NetworkBuffer sendBuffer = new NetworkBuffer();
            while (!_cts.Token.IsCancellationRequested)
            {
                sendBuffer.Reset();
                ClientSyncTimeMessage msg = ClientSyncTimeMessage.Now();
                MemoryPackSerializer.Serialize(sendBuffer, msg);
                udpClient.Send(sendBuffer.buffer, 0, sendBuffer.Position, SocketFlags.None);
                await Task.Delay(interval, _cts.Token);
            }
        }
    }
}