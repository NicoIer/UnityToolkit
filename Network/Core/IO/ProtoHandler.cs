using System;
using Google.Protobuf;

namespace Nico
{
    public static class ProtoHandler
    {
        public static class Reader<T>
        {
            public static Func<ByteString, T> reader;
        }

        public static void SetReader<T>(Func<ByteString, T> reader) => Reader<T>.reader = reader;

        public static void InitBuildInReader()
        {
            ProtoHandler.Reader<PacketHeader>.reader = PacketHeader.Parser.ParseFrom;
            ProtoHandler.Reader<ErrorMessage>.reader = ErrorMessage.Parser.ParseFrom;
            ProtoHandler.Reader<PingMessage>.reader = PingMessage.Parser.ParseFrom;
            ProtoHandler.Reader<PongMessage>.reader = PongMessage.Parser.ParseFrom;
        }
    }
}