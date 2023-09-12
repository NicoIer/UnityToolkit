using System;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nico
{
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IServerTransportGetter))]
    public class ServerManager : MonoBehaviour
    {
        #region Singleton

        public static ServerManager singleton { get; private set; }
        public bool dontDestroyOnLoad = true;
        public bool runInBackground = true;

        private bool _init_singleton()
        {
            if (!Application.isPlaying)
            {
                throw new Exception("ClientManager must be initialized in play mode");
            }

            if (singleton == this)
            {
                return true;
            }

            if (singleton != null)
            {
                Destroy(gameObject);
                return false;
            }

            if (dontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            singleton = this;
            
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
                singleton.server.Stop();
            }

            singleton = null;
        }

        #endregion

         IServerTransportGetter _getter;
        public NetServer server { get; private set; }
        public bool isRunning => server.isRunning;
        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!TryGetComponent<IServerTransportGetter>(out IServerTransportGetter getter))
            {
                Debug.LogWarning(
                    $"{nameof(ServerManager)} must has a {nameof(IServerTransportGetter)} component to get transport");
            }
        }  
#endif
        #endregion

        protected void Awake()
        {
            if (!_init_singleton()) return;
            if (singleton != this) return;
            _getter = GetComponent<IServerTransportGetter>();
            ServerTransport transport = _getter.GetServer();
            server = new NetServer(transport);
            NetworkLoop.onEarlyUpdate += OnEarlyUpdate;
            NetworkLoop.onLateUpdate += OnLateUpdate;
        }

        private void OnDestroy()
        {
            if (singleton != this) return;
            NetworkLoop.onEarlyUpdate -= OnEarlyUpdate;
            NetworkLoop.onLateUpdate -= OnLateUpdate;
            server.Stop();

            singleton = null;
        }
        
        public void OnEarlyUpdate()
        {
            server.OnEarlyUpdate();
        }
        
        public void OnLateUpdate()
        {
            server.OnLateUpdate();
        }

        /// <summary>
        /// 退出游戏时，停止网络传输，重置静态变量
        /// </summary>
        public void OnApplicationQuit()
        {
            if (singleton != this) return;
            NetStop();
            _reset_statics();
        }

        public void NetStart()
        {
            server.Start();
            Application.runInBackground = runInBackground;
        }

        public void NetStop() => server.Stop();

        public void Send<T>(int connectId, T msg, uint type = 0, int channelId = Channels.Reliable)
            where T : IMessage<T>, new() => server.Send(connectId, msg, type, channelId);

        public void SendToAll<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>, new()
            => server.SendToAll(msg, type, channelId);
    }
}