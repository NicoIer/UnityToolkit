// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Network
{
    public static class NetworkToolkit
    {
        /// <summary>        
        /// 获取操作系统已用的端口号        
        /// </summary>        
        /// <returns></returns>        
        public static IReadOnlyList<int> PortIsUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息            

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序            

            IPEndPoint[] ipsTcp = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序            

            IPEndPoint[] ipsUdp = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。            

            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            var allPorts = new List<int>();

            foreach (IPEndPoint ep in ipsTcp)

            {
                allPorts.Add(ep.Port);
            }

            foreach (IPEndPoint ep in ipsUdp)

            {
                allPorts.Add(ep.Port);
            }

            foreach (TcpConnectionInformation conn in tcpConnInfoArray)

            {
                allPorts.Add(conn.LocalEndPoint.Port);
            }

            return allPorts;
        }

        public static int GetRandomPort()
        {
            var hasUsedPort = PortIsUsed();
            int port = 0;
            bool isRandomOk = true;
            Random random = new Random((int)DateTime.Now.Ticks);
            while (isRandomOk)
            {
                port = random.Next(1024, 65535);
                isRandomOk = hasUsedPort.Contains(port);
            }

            return port;
        }
    }
}