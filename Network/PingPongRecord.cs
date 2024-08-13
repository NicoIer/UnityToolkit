namespace Network
{
    public class PingPongRecord
    {
        public ExponentialMovingAverage rtt;
        public bool pinging = false; // 是否正在Ping
        public long lastReceivePingTime => lastSendPongTime; // 上一次收到Ping消息的时间
        public long lastReceivePongTime; // 上一次收到Pong消息的时间
        public long lastSendPingTime; // 上一次发送Ping消息的时间
        public long lastSendPongTime; // 上一次发送Pong消息的时间
    }
}