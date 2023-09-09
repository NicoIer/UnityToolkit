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

        [HideInInspector] public ServerTransport transport;
        [SerializeField] KcpComponent _kcpComponent;

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

        public void OnError(int connectId, TransportError error, string msg)
        {
            Debug.Log($"NetServer:{connectId}, Error: {error} {msg}");
        }

        public void OnDisconnected(int connectId)
        {
            Debug.LogError($"NetServer:{connectId}, Disconnected");
        }

        public void OnConnected(int connectId)
        {
            Debug.Log($"NetServer:{connectId}, Connected");
        }

        public void OnDataSent(int connectId, ArraySegment<byte> data, int channel)
        {
            Debug.Log($"NetServer:{connectId}, DataSent: {data.Count} bytes");
        }

        public void OnDataReceived(int connectId, ArraySegment<byte> data, int channel)
        {
            Debug.Log($"NetServer:{connectId}, DataReceived: {data.Count} bytes");
        }

        #endregion

        #region NetLoop

        public void OnEarlyUpdate()
        {
            transport?.TickIncoming();
        }

        public void OnLateUpdate()
        {
            transport?.TickOutgoing();
        }

        #endregion


        #region Function

        public void NetStart()
        {
            transport.Start();
        }

        public void NetStop()
        {
            transport.Stop();
            transport.Shutdown();
        }

        public void Send<T>(int connectId, T msg, int channelId = Channels.Reliable) where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.WriteProto(msg);
                transport.Send(connectId, buffer.ToArraySegment(), channelId);
            }
        }

        public void SendToAll<T>(T msg, int channelId = Channels.Reliable) where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.WriteProto(msg);
                transport.SendToAll(buffer.ToArraySegment(), channelId);
            }
        }

        #endregion
    }
}