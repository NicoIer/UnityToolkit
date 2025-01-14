using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Network
{
    internal static partial class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TimeStamp()
        {
            return $"{DateTime.Now:HH:mm:ss}";
        }

        // IntToBytes version that doesn't allocate a new byte[4] each time.
        // -> important for MMO scale networking performance.
        public static void IntToBytesBigEndianNonAlloc(int value, byte[] bytes, int offset = 0)
        {
            bytes[offset + 0] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        public static int BytesToIntBigEndian(byte[] bytes)
        {
            return (bytes[0] << 24) |
                   (bytes[1] << 16) |
                   (bytes[2] << 8) |
                   bytes[3];
        }

        // Clamp so we don't have to depend on UnityEngine
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // encode 8 bits unsigned int
        public static int Encode8u(byte[] p, int offset, byte value)
        {
            p[0 + offset] = value;
            return 1;
        }

        // decode 8 bits unsigned int
        public static int Decode8u(byte[] p, int offset, out byte value)
        {
            value = p[0 + offset];
            return 1;
        }

        // encode 16 bits unsigned int (lsb)
        public static int Encode16U(byte[] p, int offset, ushort value)
        {
            p[0 + offset] = (byte)(value >> 0);
            p[1 + offset] = (byte)(value >> 8);
            return 2;
        }

        // decode 16 bits unsigned int (lsb)
        public static int Decode16U(byte[] p, int offset, out ushort value)
        {
            ushort result = 0;
            result |= p[0 + offset];
            result |= (ushort)(p[1 + offset] << 8);
            value = result;
            return 2;
        }

        // encode 32 bits unsigned int (lsb)
        public static int Encode32U(byte[] p, int offset, uint value)
        {
            p[0 + offset] = (byte)(value >> 0);
            p[1 + offset] = (byte)(value >> 8);
            p[2 + offset] = (byte)(value >> 16);
            p[3 + offset] = (byte)(value >> 24);
            return 4;
        }

        // decode 32 bits unsigned int (lsb)
        public static int Decode32U(byte[] p, int offset, out uint value)
        {
            uint result = 0;
            result |= p[0 + offset];
            result |= (uint)(p[1 + offset] << 8);
            result |= (uint)(p[2 + offset] << 16);
            result |= (uint)(p[3 + offset] << 24);
            value = result;
            return 4;
        }

        // timediff was a macro in original Kcp. let's inline it if possible.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TimeDiff(uint later, uint earlier)
        {
            return (int)(later - earlier);
        }


        // helper function to resolve host to IPAddress
        public static bool ResolveHostname(string hostname, out IPAddress[] addresses)
        {
            try
            {
                // NOTE: dns lookup is blocking. this can take a second.
                addresses = Dns.GetHostAddresses(hostname);
                return addresses.Length >= 1;
            }
            catch (SocketException exception)
            {
                NetworkLogger.Info($"Failed to resolve host: {hostname} reason: {exception}");
                addresses = null;
                return false;
            }
        }

        // if connections drop under heavy load, increase to OS limit.
        // if still not enough, increase the OS limit.
        public static void ConfigureSocketBuffers(Socket socket, int recvBufferSize, int sendBufferSize)
        {
            // log initial size for comparison.
            // remember initial size for log comparison
            int initialReceive = socket.ReceiveBufferSize;
            int initialSend = socket.SendBufferSize;

            // set to configured size
            try
            {
                socket.ReceiveBufferSize = recvBufferSize;
                socket.SendBufferSize = sendBufferSize;
            }
            catch (SocketException)
            {
                NetworkLogger.Warning(
                    $"Failed to set {socket} RecvBufSize = {recvBufferSize} SendBufSize = {sendBufferSize}");
            }


            NetworkLogger.Info(
                $"Set {socket} RecvBuf = {initialReceive}=>{socket.ReceiveBufferSize} ({socket.ReceiveBufferSize / initialReceive}x) SendBuf = {initialSend}=>{socket.SendBufferSize} ({socket.SendBufferSize / initialSend}x)");
        }

        // generate a connection hash from IP+Port.
        //
        // NOTE: IPEndPoint.GetHashCode() allocates.
        //  it calls m_Address.GetHashCode().
        //  m_Address is an IPAddress.
        //  GetHashCode() allocates for IPv6:
        //  https://github.com/mono/mono/blob/bdd772531d379b4e78593587d15113c37edd4a64/mcs/class/referencesource/System/net/System/Net/IPAddress.cs#L699
        //
        // => using only newClientEP.Port wouldn't work, because
        //    different connections can have the same port.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConnectionHash(EndPoint endPoint) =>
            endPoint.GetHashCode();

        // cookies need to be generated with a secure random generator.
        // we don't want them to be deterministic / predictable.
        // RNG is cached to avoid runtime allocations.
        [ThreadStatic] private static RNGCryptoServiceProvider _cryptoRandom;
        [ThreadStatic] private static byte[] _cryptoRandomBuffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GenerateCookie()
        {
            if (_cryptoRandom == null)
                _cryptoRandom = new RNGCryptoServiceProvider();
            if (_cryptoRandomBuffer == null)
                _cryptoRandomBuffer = new byte[4];

            _cryptoRandom.GetBytes(_cryptoRandomBuffer);
            return BitConverter.ToUInt32(_cryptoRandomBuffer, 0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NetworkQuality CalculateQuality(int rttMs)
        {
            // 1-30ms
            if (rttMs <= 30)
                return NetworkQuality.Excellent;
            // 31-60ms
            if (rttMs <= 60)
                return NetworkQuality.Good;
            // 61-100ms
            if (rttMs <= 100)
                return NetworkQuality.Normal;
            // 101-200ms
            if (rttMs <= 200)
                return NetworkQuality.Poor;
            // >200ms
            return NetworkQuality.Bad;
        }
    }
}