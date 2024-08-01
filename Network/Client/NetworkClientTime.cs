using System;

namespace Network.Client
{
    public class NetworkClientTime
    {
        public const long DefaultPingInterval = 1000;

        /// <summary>
        /// 客户端的本地时间
        /// </summary>
        public static long LocalTime => DateTime.UtcNow.Ticks;

        public const int PingWindowSize = 50; // average over 50 * 100ms = 5s
        public long LastPingTime { get; private set; }

        private ExponentialMovingAverage _rtt;
        public long rtt => (long)_rtt.Value;
        private readonly IClientSocket _socket;

        public NetworkClientTime(IClientSocket socket)
        {
            LastPingTime = LocalTime;
            _rtt = new ExponentialMovingAverage(PingWindowSize);
            _socket = socket;
        }

        /// <summary>
        /// 客户端收到服务器的Pong消息时调用，更新本地时间
        /// </summary>
        /// <param name="message"></param>
        public void OnReceivePong(ref PongMessage message)
        {
            if (message.sendPingTime > LocalTime) return;
            // client -> server -> client round trip time
            long newRtt = LocalTime - message.sendPingTime;
            _rtt.Add(newRtt);
        }

        /// <summary>
        /// 客户端收到服务器Ping消息时调用,立刻回复Pong消息
        /// </summary>
        /// <param name="pingMessage"></param>
        public void OnReceivePing(ref PingMessage pingMessage)
        {
            PongMessage pongMessage = new PongMessage(ref pingMessage);
            _socket.Send(pongMessage);
        }

        public void Update()
        {
            if (LocalTime - LastPingTime <= DefaultPingInterval) return;
            LastPingTime = LocalTime;
            PingMessage pingMessage = new PingMessage(LocalTime);
            _socket.Send(pingMessage);
        }
    }
}