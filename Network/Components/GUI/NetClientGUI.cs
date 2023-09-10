using System;
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
                NetClient.singleton.NetStart();
            }

            if (GUILayout.Button("Stop"))
            {
                NetClient.singleton.NetStop();
            }

            GUILayout.EndArea();
        }

        public float pingInterval = 1f;
        public float pingTime = 0f;

        private void Start()
        {
            NetClient.singleton.onDisconnected += () =>
            {
                Debug.Log("onDisconnected");
            };
            
            NetClient.singleton.onConnected += () =>
            {
                Debug.Log("onConnected");
            };
            kcp2k.Log.Info = Debug.Log;
        }

        private void Update()
        {
            if (!NetClient.singleton.connected) return;
            pingTime += Time.deltaTime;
            if (pingTime >= pingInterval)
            {
                pingTime = 0f;
                PingMessage ping = new PingMessage
                {
                    ClientTime = Time.unscaledTimeAsDouble
                };
                NetClient.singleton.Send(ping);
            }
        }
    }
}