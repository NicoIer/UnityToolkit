using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MemoryPack;
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


    public class ReqRspCenter
    {
        #region Server

        public delegate void ReqHandleDelegate<TReq, TRsp>(in int connectionId, in TReq message,
            out TRsp rsp, out ErrorCode errorCode, out string errorMsg);

        private delegate RspHead ReqHandler(in int connectionId, in ReqHead head);

        private readonly Dictionary<ushort, ReqHandler> _handlers = new Dictionary<ushort, ReqHandler>();

        public RspHead Handle(in int connectionId, in ReqHead head)
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

        #endregion

        public delegate void OnResponseHandleDelegate();

        private readonly Dictionary<ushort, OnResponseHandleDelegate> _requesting = new();

        private readonly List<ushort> _idPool = new List<ushort>();
        private ushort _currentId = 0;

        public void PackRequest<TReq>(in TReq req, out ReqHead reqHead) where TReq : INetworkReq
        {
            ushort reqHash = TypeId<TReq>.stableId16;

            if (_idPool.Count == 0)
            {
                _idPool.Add(_currentId++);
            }

            var reqIndex = _idPool[^1];

            _idPool.RemoveAt(_idPool.Count - 1);

            reqHead = new ReqHead
            {
                reqHash = reqHash,
                index = reqIndex,
                payload = MemoryPackSerializer.Serialize(req),
            };
        }

        public void OnResponse(in ushort index, in RspHead rspHead)
        {
            if (rspHead.error != ErrorCode.Success)
            {
                ToolkitLog.Warning($"收到了一个发生错误的响应:{rspHead.error},{rspHead.errorMessage}");
                return;
            }

            if (!_requesting.TryGetValue(index, out var value))
            {
                ToolkitLog.Warning($"收到了一个未请求的响应:{rspHead}");
                return;
            }

            value();
            _requesting.Remove(index);
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
            this.payload = payload;
        }
    }
}