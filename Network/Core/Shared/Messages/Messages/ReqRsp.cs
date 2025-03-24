using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MemoryPack;
using Network.Client;
using UnityToolkit;

namespace Network
{
    // [MemoryPackable]
    // public partial struct NullObj : INetworkMessage
    // {
    //     public ushort objTypeId;
    //     public ArraySegment<byte> payload;
    // }
    //
    // [MemoryPackable]
    // public partial struct Error : INetworkMessage
    // {
    // }


    public class ReqRspServerCenter
    {
        public delegate void ReqHandleDelegate<TReq, TRsp>(in int connectionId, in TReq message,
            out TRsp rsp, out ErrorCode errorCode, out string errorMsg);

        private delegate RspHead ReqHandler(in int connectionId, in ReqHead head);

        private readonly Dictionary<ushort, ReqHandler> _handlers = new Dictionary<ushort, ReqHandler>();

        public RspHead HandleRequest(in int connectionId, in ReqHead head)
        {
            if (_handlers.TryGetValue(head.reqHash, out var handler))
            {
                return handler(connectionId, in head);
            }

            return new RspHead(head.index, head.reqHash, 0, ErrorCode.NotSupported, null, default);
        }

        public void Register<TReq, TRsp>(ReqHandleDelegate<TReq, TRsp> handleDelegate)
            where TReq : INetworkReq
            where TRsp : INetworkRsp
        {
            ushort reqHash = TypeId<TReq>.stableId16;
            ushort rspHash = TypeId<TRsp>.stableId16;
            _handlers[reqHash] = (in int connectionId, in ReqHead head) =>
            {
                var req = MemoryPackSerializer.Deserialize<TReq>(head.payload);
                handleDelegate(connectionId, req, out var rsp, out var errorCode, out var errorMsg);
                var payload = MemoryPackSerializer.Serialize(rsp);
                return new RspHead(head.index, reqHash, rspHash, errorCode, errorMsg, payload);
            };
        }

    }
    


    public enum ErrorCode
    {
        Success,
        InvalidArgument,
        InternalError,
        NotSupported,
        Timeout,
    }

    public interface INetworkReq
    {
    }

    public interface INetworkRsp
    {
    }


    [MemoryPackable]
    public partial struct ReqHead : INetworkMessage
    {
        public ushort reqHash;
        public ushort index;
        public ArraySegment<byte> payload;
    }


    [MemoryPackable]
    public partial struct RspHead : INetworkMessage
    {
        public readonly ushort index;
        public readonly ushort reqHash;
        public readonly ushort rspHash;
        public readonly ErrorCode error;
        public readonly string errorMessage;
        public readonly ArraySegment<byte> payload;

        public RspHead(ushort index, ushort reqHash, ushort rspHash, ErrorCode error, in string errorMessage,
            ArraySegment<byte> payload)
        {
            this.index = index;
            this.reqHash = reqHash;
            this.rspHash = rspHash;
            this.error = error;
            this.errorMessage = errorMessage;
            this.payload = payload;
        }
    }
}