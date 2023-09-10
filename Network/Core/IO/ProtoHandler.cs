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
    }
}