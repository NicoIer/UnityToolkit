using System;
using Google.Protobuf;

namespace Nico
{
    public static class ProtoReader
    {
        public static class Reader<T>
        {
            public static Func<ByteString, T> reader;
        }

        static ProtoReader()
        {
            Reader<PacketHeader>.reader = PacketHeader.Parser.ParseFrom;
            Reader<ErrorMessage>.reader = ErrorMessage.Parser.ParseFrom;
            Reader<PingMessage>.reader = PingMessage.Parser.ParseFrom;;
            Reader<PongMessage>.reader = PongMessage.Parser.ParseFrom;
        }
    }
}