// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
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
        
        public bool isRunning => _cts is { IsCancellationRequested: false };

        /// <summary>
        /// 估算出来的服务器时间
        /// </summary>
        public long serverTimeMs => lastServerMs + _stopwatch.ElapsedMilliseconds;

        private ExponentialMovingAverage _rttEma;
        public const int RttEmaSize = 16;
        private ExponentialMovingAverage _serverTimeEma;

        public const int ServerTimeEmaSize = 16;

        // public double standardDeviation => _serverTimeEma.StandardDeviation;
        private double _toleranceMs;
        public bool reachingAccuracy => _serverTimeEma.StandardDeviation < _toleranceMs;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toleranceMs">对时误差小于这个值的时候认为对时准备</param>
        public NetworkTimeClient(double toleranceMs = 1)
        {
            _toleranceMs = toleranceMs;
            _stopwatch = new Stopwatch();
            _rttEma = new ExponentialMovingAverage(RttEmaSize);
            _serverTimeEma = new ExponentialMovingAverage(ServerTimeEmaSize);
        }

        private bool _autoStop;

        private IPEndPoint _remoteEP;

        public async Task Run(string ip, int port, bool autoStop = false, float intervalSeconds = 1.0f)
        {
            this._autoStop = autoStop;
            Debug.Assert(_cts == null, "NetworkTimeClient is already running");
            _cts = new CancellationTokenSource();

            IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
            _remoteEP = point;
            var udpClient =
                // new UdpClient(ip, port);
                new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
// #if DEBUG || UNITY_EDITOR
// #endif
            await udpClient.ConnectAsync(point);

            ToolkitLog.Info($"{nameof(NetworkTimeClient)} connected to {ip}:{port}");
            Task sendTask = SendTask(udpClient, intervalSeconds);
            Task receiveTask = ReceiveTask(udpClient);

            await Task.WhenAll(sendTask, receiveTask);
        }

        public void Stop()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }


        private async Task ReceiveTask(Socket udpClient)
        {
            byte[] receiveBuffer = new byte[1024];
            Memory<byte> receiveSegment = new Memory<byte>(receiveBuffer);
            while (!_cts.Token.IsCancellationRequested)
            {
                int length =
                    await udpClient.ReceiveAsync(receiveSegment, SocketFlags.None, _cts.Token);
                // udpClient.Receive(receiveBuffer);

                ServerSyncTimeMessage serverSyncTimeMessage =
                    MemoryPackSerializer.Deserialize<ServerSyncTimeMessage>(
                        new ReadOnlySpan<byte>(receiveBuffer, 0, length));

                long clientSendMs = serverSyncTimeMessage.clientSendMs;
                long serverReceiveMs = serverSyncTimeMessage.serverReceiveMs;
                long nowMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                _rttEma.Add(nowMs - clientSendMs);
                _stopwatch.Restart();
                
                // ToolkitLog.Info(_rttEma.Value.ToString("F"));
                
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
                    if (_autoStop)
                    {
                        ToolkitLog.Info("网络对时误差达到容忍度 停止对时");
                        Stop();
                    }

                    await Task.Delay(interval, _cts.Token);
                }

                try
                {
                    sendBuffer.Reset();
                    ClientSyncTimeMessage msg = ClientSyncTimeMessage.Now();
                    MemoryPackSerializer.Serialize(sendBuffer, msg);
                    await udpClient.SendAsync(sendBuffer, SocketFlags.None, _cts.Token);
                    // ToolkitLog.Debug("向服务器发送对时请求");
                    // udpClient.Send(sendBuffer.buffer, 0, sendBuffer.Position, SocketFlags.None);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        ToolkitLog.Warning("对时服务器拒绝连接 重新连接");
                        await udpClient.ConnectAsync(_remoteEP);
                        continue;
                    }

                    throw;
                }

                await Task.Delay(interval, _cts.Token);
            }
        }
    }
}