using System;
using System.Threading;
using Google.Protobuf;
using UnityEngine;

namespace Nico
{
    public class NetClientGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            //右上角绘制启动等信息
            GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 200));
            if (GUILayout.Button("Start"))
            {
                ClientManager.singleton.NetStart();
            }

            if (GUILayout.Button("Stop"))
            {
                ClientManager.singleton.NetStop();
            }

            GUILayout.EndArea();
        }

        public float pingInterval = 1f;
        public float pingTime = 0f;

        private void Start()
        {
            // kcp2k.Log.Info = Debug.Log;

            ClientManager.singleton.client.OnDisconnected += () => { Debug.Log("onDisconnected"); };

            ClientManager.singleton.client.OnConnected += () => { Debug.Log("onConnected"); };

            ClientManager.singleton.client.Register<PongMessage>(OnPong);
        }

        public void OnPong(PongMessage pongMessage, int channelId)
        {
            long delta = DateTime.Now.ToUniversalTime().Ticks - pongMessage.ServerTime;
            double ms = delta / 10000f;
            Debug.Log($"pong message from {channelId} delta:{delta} = {ms:0000} ms");
        }

        private void Update()
        {
            if (!ClientManager.singleton.connected) return;
            pingTime += Time.deltaTime;
            if (pingTime >= pingInterval)
            {
                pingTime = 0f;
                PingMessage ping = ProtoHandler.Get<PingMessage>();
                ping.ClientTime = DateTime.Now.ToUniversalTime().Ticks;
                ClientManager.singleton.Send(ping);
                ping.Return();
            }
        }
    }
}