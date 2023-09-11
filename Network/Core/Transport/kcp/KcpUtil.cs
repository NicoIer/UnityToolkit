using System;
using kcp2k;

namespace Nico
{
    public static class KcpUtil
    {
        public static readonly KcpConfig defaultConfig = new KcpConfig(
            true, //同时监听 ipv4 和 ipv6
            1024 * 1024 * 7,
            1024 * 1024 * 7,
            Kcp.MTU_DEF,
            true,
            10,
            2,
            false,
            4096,
            4096,
            10000,
            Kcp.DEADLINK * 2
        );
        public static int FromKcpChannel(KcpChannel channel) =>
            channel == KcpChannel.Reliable ? Channels.Reliable : Channels.Unreliable;

        public static KcpChannel ToKcpChannel(int channel) =>
            channel == Channels.Reliable ? KcpChannel.Reliable : KcpChannel.Unreliable;

        public static TransportError ToTransportError(ErrorCode error)
        {
            switch(error)
            {
                case ErrorCode.DnsResolve: return TransportError.DnsResolve;
                case ErrorCode.Timeout: return TransportError.Timeout;
                case ErrorCode.Congestion: return TransportError.Congestion;
                case ErrorCode.InvalidReceive: return TransportError.InvalidReceive;
                case ErrorCode.InvalidSend: return TransportError.InvalidSend;
                case ErrorCode.ConnectionClosed: return TransportError.ConnectionClosed;
                case ErrorCode.Unexpected: return TransportError.Unexpected;
                default: throw new InvalidCastException($"KCP: missing error translation for {error}");
            }
        }
        
        
        
    }
}