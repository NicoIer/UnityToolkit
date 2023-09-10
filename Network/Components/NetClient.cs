using System;
using Google.Protobuf;
using UnityEngine;

namespace Nico
{
    [DisallowMultipleComponent]
    public class NetClient : MonoBehaviour
    {
        #region Singleton

        public static NetClient singleton { get; private set; }
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


        public ClientTransport transport { get; private set; }
        [SerializeField] KcpComponent _kcpComponent;
        public string address = "localhost";
        public ClientCenter center { get; private set; } = new ClientCenter();
        public bool connected => transport.connected;

        #region Net Events

        public event Action onDisconnected;
        public event Action onConnected;

        public event Action<TransportError, string> onError;

        public event Action<ArraySegment<byte>, int> onDataReceived;

        public event Action<ArraySegment<byte>, int> onDataSent;

        #endregion

        protected void Awake()
        {
            if (!_init_singleton()) return;
            if (singleton != this) return;
            _kcpComponent = GetComponent<KcpComponent>();
            transport = _kcpComponent.GetClient();

            NetworkLoop.onEarlyUpdate += OnEarlyUpdate;
            NetworkLoop.onLateUpdate += OnLateUpdate;

            transport.OnError += OnError;
            transport.OnDisconnected += OnDisconnected;
            transport.OnConnected += OnConnected;
            transport.OnDataReceived += OnDataReceived;
            transport.OnDataSent += OnDataSent;
        }

        #region Transport Event

        private void OnDataSent(ArraySegment<byte> data, int channelId)
        {
            onDataSent?.Invoke(data, channelId);
        }

        private void OnDataReceived(ArraySegment<byte> data, int channelId)
        {
            center.OnData(PacketHeader.Parser.ParseFrom(data), channelId);
            onDataReceived?.Invoke(data, channelId);
        }

        private void OnConnected()
        {
            onConnected?.Invoke();
        }

        private void OnError(TransportError error, string msg)
        {
            onError?.Invoke(error, msg);
        }

        private void OnDisconnected()
        {
            onDisconnected?.Invoke();
        }

        #endregion


        #region NetLoop

        public void NetStart()
        {
            transport.Connect(address);
            Application.runInBackground = runInBackground;
        }


        public void NetStop()
        {
            transport.Disconnect();
            transport.Shutdown();
        }

        private void OnEarlyUpdate()
        {
            transport?.TickIncoming();
        }

        private void OnLateUpdate()
        {
            transport?.TickOutgoing();
        }

        private void OnDestroy()
        {
            if (singleton != this) return;
            NetworkLoop.onEarlyUpdate -= OnEarlyUpdate;
            NetworkLoop.onLateUpdate -= OnLateUpdate;
            transport.Disconnect();
            transport.Shutdown();

            singleton = null;
        }

        #endregion

        #region Function

        public void Send<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>
        {
            // 拿两个buffer 一个用来写头 一个用来写body
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.Pack(msg, type, channelId);
                transport.Send(buffer.ToArraySegment(), channelId);
            }
        }

        #endregion
    }
}