// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityToolkit;

namespace Network
{
    public sealed class LocalNetwork
    {
        // 更可靠的本地IP获取方法
        public static string GetLocalIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 只获取已启动且支持IPv4的接口
                if (ni.OperationalStatus != OperationalStatus.Up ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // 排除自动配置地址 (169.254.x.x)
                        if (!ip.Address.ToString().StartsWith("169.254."))
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }

            return "127.0.0.1";
        }
        //
        // // 获取广播地址
        // public static IPAddress GetBroadcastAddress()
        // {
        //     foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        //     {
        //         if (ni.OperationalStatus != OperationalStatus.Up ||
        //             ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
        //             continue;
        //
        //         foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
        //         {
        //             if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
        //             {
        //                 // 排除自动配置地址
        //                 if (ip.Address.ToString().StartsWith("169.254."))
        //                     continue;
        //
        //                 // 计算广播地址
        //                 IPAddress address = ip.Address;
        //                 IPAddress mask = ip.IPv4Mask;
        //
        //                 if (mask != null)
        //                 {
        //                     byte[] ipBytes = address.GetAddressBytes();
        //                     byte[] maskBytes = mask.GetAddressBytes();
        //                     byte[] broadcastBytes = new byte[ipBytes.Length];
        //
        //                     for (int i = 0; i < ipBytes.Length; i++)
        //                     {
        //                         broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
        //                     }
        //
        //                     return new IPAddress(broadcastBytes);
        //                 }
        //             }
        //         }
        //     }
        //
        //
        //     return null;
        // }

        private UdpClient _udpReceiver;
        private UdpClient _udpSender;

        public delegate void DataReceivedHandler(in ArraySegment<byte> data, in IPEndPoint sender);

        public delegate bool TickDataGenerator(out byte[] data);

        private DataReceivedHandler _onDataReceived;
        private TickDataGenerator _generator;
        public bool Running { get; private set; } = false;
        private CancellationTokenSource _cts;
        private readonly int _port;
        public int FrameRate { get; private set; }

        public LocalNetwork(int port, TickDataGenerator generator, DataReceivedHandler handler, int frameRate = 30)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            _onDataReceived = handler;
            _generator = generator;
            FrameRate = frameRate;
            _port = port;
        }


        public async Task Start()
        {
            if (Running) throw new InvalidOperationException("Network is already running.");
            Running = true;
            _cts = new CancellationTokenSource();
            var broadcastLoop = Task.Run(BroadcastLoop, _cts.Token);
            var receiveLoop = Task.Run(ReceiveLoop, _cts.Token);
            ToolkitLog.Info("LocalNetwork started.");

            await Task.WhenAll(broadcastLoop, receiveLoop);
            _udpReceiver?.Close();
            _udpSender?.Close();
            Running = false;
            ToolkitLog.Info("LocalNetwork stopped.");
        }

        public void Stop()
        {
            if (!Running) throw new InvalidOperationException("Network is not running.");
            _cts.Cancel(); // Cancel the running tasks
        }

        private async Task ReceiveLoop()
        {
            if (_onDataReceived == null) return;
            using (_udpReceiver = new UdpClient(_port))
            {
                _udpReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _udpReceiver.ReceiveAsync();
                        // 处理接收到的消息
                        _onDataReceived(result.Buffer, result.RemoteEndPoint);
                    }
                    catch (ObjectDisposedException)
                    {
                        /* 正常关闭 */
                    }
                    catch (Exception ex)
                    {
                        ToolkitLog.Error($"{this}:接收数据时发生错误: {ex.Message}");
                    }
                }
            }
        }


        private async Task BroadcastLoop()
        {
            if (_generator == null) return;
            using (_udpSender = new UdpClient())
            {
                _udpSender.EnableBroadcast = true;
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_generator(out var data))
                        {
                            // 发送数据
                            await _udpSender.SendAsync(data, data.Length,
                                new IPEndPoint(IPAddress.Broadcast, _port));
                            // ToolkitLog.Info($"已广播: {data.Length} 个字节 [{DateTime.Now:T}]");
                        }

                        await Task.Delay(1000 / FrameRate, _cts.Token); // 控制帧率
                    }
                    catch (Exception ex)
                    {
                        // 处理发送错误
                        ToolkitLog.Error($"{this}:广播数据时发生错误: {ex.Message}");
                    }
                }
            }
        }
    }
}