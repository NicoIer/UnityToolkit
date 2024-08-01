using System;
using System.Collections.Generic;
using Network.Client;

namespace Network.Server
{
    public class NetworkServerTime
    {
        public static long LocalTime => DateTime.UtcNow.Ticks;
        private IServerSocket _socket;
        private NetworkBufferPool _bufferPool;
        private Dictionary<int, ExponentialMovingAverage> _rtt;

        public NetworkServerTime(IServerSocket socket, NetworkBufferPool bufferPool)
        {
            _socket = socket;
            _bufferPool = bufferPool;
            _rtt = new Dictionary<int, ExponentialMovingAverage>(16);
        }

        public void OnDisconnected(int connectionId)
        {
            if (_rtt.ContainsKey(connectionId))
            {
                _rtt.Remove(connectionId);
            }
        }

        public void OnPong(int connectionId, PongMessage pong)
        {
        }

        public void OnPing(int connectionId, PingMessage ping)
        {
        }
    }
}