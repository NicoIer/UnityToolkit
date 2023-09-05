using System;
using Sirenix.OdinInspector;
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

        [Button]
        public void SendInt()
        {
            NetClient.singleton.Send(123);
        }
    }
}