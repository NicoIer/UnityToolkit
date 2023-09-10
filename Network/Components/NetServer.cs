using System;
using Google.Protobuf;
using UnityEngine;

namespace Nico
{
    [DisallowMultipleComponent]
    public class NetServer : MonoBehaviour
    {
        #region Singleton

        public static NetServer singleton { get; private set; }
        public bool dontDestroyOnLoad = true;
        public bool runInBackground = true;

        private bool _init_singleton()
        {
            if (singleton != null && singleton == this)
            {
                return true;
            }

            if (dontDestroyOnLoad)
            {
                if (singleton != null)
                {
                    Destroy(gameObject);
                    return false;
                }

                singleton = this;
                if (Application.isPlaying)
                {
                    transform.SetParent(null);
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                singleton = this;
            }

            return true;
        }

        /// <summary>
        /// 进入游戏前 重置上一次游戏的网络状态
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void _reset_statics()
        {
            if (singleton != null)
            {
                singleton.NetStop();
            }

            singleton = null;
        }

        /// <summary>
        /// 退出游戏时，停止网络传输，重置静态变量
        /// </summary>
        public void OnApplicationQuit()
        {
            NetStop();
            _reset_statics();
        }

        #endregion

        public ServerTransport transport { get; private set; }
        [SerializeField] KcpComponent _kcpComponent;


        public ServerCenter center { get; private set; } = new ServerCenter();
        public bool isRunning => transport.Active();

        #region Net Events

        public event Action<int, TransportError, string> onError;
        public event Action<int> onDisconnected;
        public event Action<int> onConnected;

        public Action<int, ArraySegment<byte>, int> onDataReceived;

        public Action<int, ArraySegment<byte>, int> onDataSent;

        #endregion

        protected void Awake()
        {
            if (!_init_singleton()) return;
            if (singleton != this) return;
            _kcpComponent = GetComponent<KcpComponent>();
            transport = _kcpComponent.GetServer();

            NetworkLoop.onEarlyUpdate += OnEarlyUpdate;
            NetworkLoop.onLateUpdate += OnLateUpdate;

            transport.OnError += OnError;
            transport.OnDisconnected += OnDisconnected;
            transport.OnConnected += OnConnected;
            transport.OnDataSent += OnDataSent;
            transport.OnDataReceived += OnDataReceived;
        }

        #region Transport Event

        private void OnError(int connectId, TransportError error, string msg)
        {
            onError?.Invoke(connectId, error, msg);
        }

        private void OnDisconnected(int connectId)
        {
            onDisconnected?.Invoke(connectId);
        }

        private void OnConnected(int connectId)
        {
            onConnected?.Invoke(connectId);
        }

        private void OnDataSent(int connectId, ArraySegment<byte> data, int channel)
        {
            onDataSent?.Invoke(connectId, data, channel);
        }

        private void OnDataReceived(int connectId, ArraySegment<byte> data, int channel)
        {
            center.OnData(connectId, PacketHeader.Parser.ParseFrom(data), channel);
            onDataReceived?.Invoke(connectId, data, channel);
        }

        #endregion

        #region NetLoop

        public void NetStart()
        {
            transport.Start();
            Application.runInBackground = runInBackground;
        }

        public void NetStop()
        {
            transport.Stop();
            transport.Shutdown();
        }

        public void OnEarlyUpdate()
        {
            transport?.TickIncoming();
        }

        public void OnLateUpdate()
        {
            transport?.TickOutgoing();
        }

        private void OnDestroy()
        {
            if (singleton != this) return;
            NetworkLoop.onEarlyUpdate -= OnEarlyUpdate;
            NetworkLoop.onLateUpdate -= OnLateUpdate;
            transport.Shutdown();

            singleton = null;
        }

        #endregion


        #region Function

        public void Send<T>(int connectId, T msg, uint type = 0, int channelId = Channels.Reliable)
            where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.Pack(msg, type, channelId);
                transport.Send(connectId, buffer.ToArraySegment(), channelId);
            }
        }

        public void SendToAll<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.Pack(msg, type, channelId);
                transport.SendToAll(buffer.ToArraySegment(), channelId);
            }
        }

        #endregion
    }
}