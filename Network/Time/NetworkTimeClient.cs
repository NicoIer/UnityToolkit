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

        /// <summary>
        /// 上一次对时获得的服务器时间
        /// </summary>
        private long lastServerMs;

        /// <summary>
        /// 估算出的RTT
        /// </summary>
        public double rttMs => _rttEma.Value;

        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// 估算出来的服务器时间
        /// </summary>
        public long serverTimeMs => lastServerMs + _stopwatch.ElapsedMilliseconds;

        private ExponentialMovingAverage _rttEma;
        public const int RttEmaSize = 4;
        private ExponentialMovingAverage _serverTimeEma;

        public const int ServerTimeEmaSize = 8;

        // public double standardDeviation => _serverTimeEma.StandardDeviation;
        private double _toleranceMs;
        public bool reachingAccuracy => _serverTimeEma.StandardDeviation < _toleranceMs;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toleranceMs">对时误差小于这个值的时候认为对时准备</param>
        public NetworkTimeClient(double toleranceMs = 10)
        {
            _toleranceMs = toleranceMs;
            _stopwatch = new Stopwatch();
            _rttEma = new ExponentialMovingAverage(RttEmaSize);
            _serverTimeEma = new ExponentialMovingAverage(ServerTimeEmaSize);
        }


        public Task Run(string ip, int port, float intervalSeconds = 1.0f)
        {
            Debug.Assert(_cts == null, "NetworkTimeClient is already running");
            _cts = new CancellationTokenSource();

            IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpClient.Connect(point);

            Task sendTask = SendTask(udpClient, intervalSeconds);
            Task receiveTask = ReceiveTask(udpClient);

            return Task.WhenAll(sendTask, receiveTask);
        }

        public void Stop()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
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
                _stopwatch.Restart();
                long beforeServerMs = lastServerMs;
                lastServerMs = (long)(serverReceiveMs + rttMs / 2);

                // 计算服务器时间的EMA 如果对时非常准确 这里应该是0
                _serverTimeEma.Add(lastServerMs - beforeServerMs);

                // 毫秒级别的标准差
                // var standardDeviation = _serverTimeEma.StandardDeviation;


                await Task.Yield();
            }
        }

        private async Task SendTask(Socket udpClient, float intervalSeconds)
        {
            TimeSpan interval = TimeSpan.FromSeconds(intervalSeconds);
            NetworkBuffer sendBuffer = new NetworkBuffer();
            while (!_cts.Token.IsCancellationRequested)
            {
                if (reachingAccuracy)
                {
                    Stop();
                }

                try
                {
                    sendBuffer.Reset();
                    ClientSyncTimeMessage msg = ClientSyncTimeMessage.Now();
                    MemoryPackSerializer.Serialize(sendBuffer, msg);
                    udpClient.Send(sendBuffer.buffer, 0, sendBuffer.Position, SocketFlags.None);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                await Task.Delay(interval, _cts.Token);
            }
        }
    }
}