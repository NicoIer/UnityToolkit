using System;
using Network.Client;
using UnityToolkit;

namespace Network
{
    public sealed class NetworkTimeClient
    {
        private NetworkClient _client;
        public long rttMs { get; private set; }
        public long serverTimeMs { get; private set; }
        public bool connected => _client.socket.connected;

        private readonly ICommand _removeOnPingMessage;
        private readonly ICommand _removeOnTimestampMessage;

        public DateTimeOffset Now => DateTimeOffset.FromUnixTimeMilliseconds(serverTimeMs);

        public NetworkTimeClient(NetworkClient client)
        {
            _client = client;
            _removeOnPingMessage = client.messageHandler.Add<PingMessage>(OnPingMessage);
            _removeOnTimestampMessage = client.messageHandler.Add<ServerTimestampMessage>(OnTimestampMessage);
        }

        ~NetworkTimeClient()
        {
            _removeOnPingMessage.Execute();
            _removeOnTimestampMessage.Execute();
        }


        private void OnTimestampMessage(ServerTimestampMessage serverTimestamp)
        {
            rttMs = serverTimestamp.serverAssumeRttMs;
            serverTimeMs = serverTimestamp.serverSendMs + rttMs / 2;
        }

        private void OnPingMessage(PingMessage ping)
        {
            _client.Send(new PongMessage(ref ping));
        }
    }
}