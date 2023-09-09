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


        [HideInInspector] public ClientTransport transport;
        [SerializeField] KcpComponent _kcpComponent;
        public string address = "localhost";


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

        public void OnDataSent(ArraySegment<byte> data, int channelId)
        {
        }

        public void OnDataReceived(ArraySegment<byte> data, int channelId)
        {
            Debug.Log($"NetClient Received: {data.Count} bytes");
            PacketHeader header = PacketHeader.Parser.ParseFrom(data);
            Debug.Log(header.Id);
            StringMessage stringMessage = StringMessage.Parser.ParseFrom(header.Body);
            Debug.Log(stringMessage.Msg);
        }

        public void OnConnected()
        {
            Debug.Log($"NetClient Connected");
        }

        public void OnError(TransportError error, string msg)
        {
            Debug.LogError($"NetClient Error: {error} {msg}");
        }

        public void OnDisconnected()
        {
        }

        #endregion

        private void OnEarlyUpdate()
        {
            transport?.TickIncoming();
        }

        private void OnLateUpdate()
        {
            transport?.TickOutgoing();
        }


        public void NetStart()
        {
            transport.Connect(address);
        }


        public void NetStop()
        {
            transport.Disconnect();
            transport.Shutdown();
        }


        public void Send<T>(T msg, int channelId = Channels.Reliable) where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.WriteProto(msg);
                transport.Send(buffer, channelId);
            }
        }
    }
}