using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    internal static class Extensions
    {
        // ArraySegment as HexString for convenience
        public static string ToHexString(this ArraySegment<byte> segment) =>
            BitConverter.ToString(segment.Array, segment.Offset, segment.Count);

        // non-blocking UDP send.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public static bool SendToNonBlocking(this Socket socket, ArraySegment<byte> data, EndPoint remoteEP)
        {
            try
            {
                // when using non-blocking sockets, SendTo may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!socket.Poll(0, SelectMode.SelectWrite)) return false;

                // send to the the endpoint.
                // do not send to 'newClientEP', as that's always reused.
                // fixes https://github.com/MirrorNetworking/Mirror/issues/3296
                socket.SendTo(data.Array, data.Offset, data.Count, SocketFlags.None, remoteEP);
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, SendTo may throw WouldBlock.
                // in that case, simply drop the message. it's UDP, it's fine.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }

        // non-blocking UDP send.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public static bool SendNonBlocking(this Socket socket, ArraySegment<byte> data)
        {
            try
            {
                // when using non-blocking sockets, SendTo may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!socket.Poll(0, SelectMode.SelectWrite)) return false;

                // SendTo allocates. we used bound Send.
                socket.Send(data.Array, data.Offset, data.Count, SocketFlags.None);
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, SendTo may throw WouldBlock.
                // in that case, simply drop the message. it's UDP, it's fine.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }

        // non-blocking UDP receive.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public static bool ReceiveFromNonBlocking(this Socket socket, byte[] recvBuffer, out ArraySegment<byte> data, ref EndPoint remoteEP)
        {
            data = default;

            try
            {
                // when using non-blocking sockets, ReceiveFrom may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!socket.Poll(0, SelectMode.SelectRead)) return false;

                // NOTE: ReceiveFrom allocates.
                //   we pass our IPEndPoint to ReceiveFrom.
                //   receive from calls newClientEP.Create(socketAddr).
                //   IPEndPoint.Create always returns a new IPEndPoint.
                //   https://github.com/mono/mono/blob/f74eed4b09790a0929889ad7fc2cf96c9b6e3757/mcs/class/System/System.Net.Sockets/Socket.cs#L1761
                //
                // throws SocketException if datagram was larger than buffer.
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receive?view=net-6.0
                int size = socket.ReceiveFrom(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, ref remoteEP);
                data = new ArraySegment<byte>(recvBuffer, 0, size);
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, Receive throws WouldBlock if there is
                // no message to read. that's okay. only log for other errors.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }

        // non-blocking UDP receive.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public static bool ReceiveNonBlocking(this Socket socket, byte[] recvBuffer, out ArraySegment<byte> data)
        {
            data = default;

            try
            {
                // when using non-blocking sockets, ReceiveFrom may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!socket.Poll(0, SelectMode.SelectRead)) return false;

                // ReceiveFrom allocates. we used bound Receive.
                // returns amount of bytes written into buffer.
                // throws SocketException if datagram was larger than buffer.
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receive?view=net-6.0
                //
                // throws SocketException if datagram was larger than buffer.
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receive?view=net-6.0
                int size = socket.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None);
                data = new ArraySegment<byte>(recvBuffer, 0, size);
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, Receive throws WouldBlock if there is
                // no message to read. that's okay. only log for other errors.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }
        
        

        // string.GetHashCode is not guaranteed to be the same on all
        // machines, but we need one that is the same on all machines.
        // Uses fnv1a as hash function for more uniform distribution http://www.isthe.com/chongo/tech/comp/fnv/
        // Tests: https://softwareengineering.stackexchange.com/questions/49550/which-hashing-algorithm-is-best-for-uniqueness-and-speed
        // NOTE: Do not call this from hot path because it's slow O(N) for long method names.
        // - As of 2012-02-16 There are 2 design-time callers (weaver) and 1 runtime caller that caches.
        internal static int GetStableHashCode(this string text)
        {
            unchecked
            {
                uint hash = 0x811c9dc5;
                uint prime = 0x1000193;

                for (int i = 0; i < text.Length; ++i)
                {
                    byte value = (byte)text[i];
                    hash = hash ^ value;
                    hash *= prime;
                }

                //UnityEngine.Debug.Log($"Created stable hash {(ushort)hash} for {text}");
                return (int)hash;
            }
        }

        // smaller version of our GetStableHashCode.
        // careful, this significantly increases chance of collisions.
        internal static ushort GetStableHashCode16(this string text)
        {
            // deterministic hash
            int hash = GetStableHashCode(text);

            // Gets the 32bit fnv1a hash
            // To get it down to 16bit but still reduce hash collisions we cant just cast it to ushort
            // Instead we take the highest 16bits of the 32bit hash and fold them with xor into the lower 16bits
            // This will create a more uniform 16bit hash, the method is described in:
            // http://www.isthe.com/chongo/tech/comp/fnv/ in section "Changing the FNV hash size - xor-folding"
            return (ushort)((hash >> 16) ^ hash);
        }
    }
}