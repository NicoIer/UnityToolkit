using System;
using Network.Client;
using UnityToolkit;

namespace Network
{
    public sealed class NetworkTimeClient
    {
        private readonly NetworkClientMessageHandler _messageHandler;
        private IClientSocket _socket;
        public long rttMs { get; private set; }
        public long serverTimeMs { get; private set; }
        public bool connected => _socket.connected;

        public NetworkTimeClient(IClientSocket socket)
        {
            _socket = socket;
            _socket.OnDataReceived += OnData;

            _messageHandler = new NetworkClientMessageHandler();
            _messageHandler.Add<PingMessage>(OnPingMessage);
            _messageHandler.Add<ServerTimestampMessage>(OnTimestampMessage);
        }

        ~NetworkTimeClient()
        {
            _socket.OnDataReceived -= OnData;
        }


        private void OnData(ArraySegment<byte> obj)
        {
            NetworkPacker.Unpack(obj, out var message);
            _messageHandler.Handle(message.id, message.payload);
        }

        public DateTimeOffset Now => DateTimeOffset.FromUnixTimeMilliseconds(serverTimeMs);

        private void OnTimestampMessage(ServerTimestampMessage serverTimestamp)
        {
            rttMs = serverTimestamp.serverAssumeRttMs;
            serverTimeMs = (long)(serverTimestamp.serverSendMs + rttMs / 2.0f);
#if DEBUG
            ToolkitLog.Debug(
                $"NetworkClientTime->OnTime: RTT: {rttMs}ms ServerTime: {DateTimeOffset.FromUnixTimeMilliseconds(serverTimeMs):h:mm:ss tt zz}");
#endif
        }

        private void OnPingMessage(PingMessage ping)
        {
            NetworkBuffer payloadBuffer = NetworkBufferPool.Shared.Get();
            NetworkBuffer packetBuffer = NetworkBufferPool.Shared.Get();

            PongMessage pongMessage = new PongMessage(ref ping);
            NetworkPacker.Pack(in pongMessage, payloadBuffer, packetBuffer);
            _socket.Send(packetBuffer);

            NetworkBufferPool.Shared.Return(payloadBuffer);
            NetworkBufferPool.Shared.Return(packetBuffer);
        }
    }
}