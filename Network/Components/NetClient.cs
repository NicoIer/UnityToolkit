using System;
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
        }

        public void Send<T>(T msg, int channelId = Channels.Reliable)
        {
            using (NetWriter writer = NetWriter.Get())
            {
                writer.Write(msg);
                transport.Send(writer, channelId);
            }
        }

        #region Transport Event

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
            Debug.Log($"NetClient Disconnected");
        }

        #endregion

        public void OnEarlyUpdate()
        {
            transport?.TickIncoming();
        }

        public void OnLateUpdate()
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

        private void OnGUI()
        {
            if (singleton != this) return;
            //右上角绘制启动等信息
            GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 200));
            if (GUILayout.Button("Start"))
            {
                NetStart();
            }

            if (GUILayout.Button("Stop"))
            {
                NetStop();
            }

            GUILayout.EndArea();
        }
    }
}