using System;
using Google.Protobuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nico
{
    public class NetServerGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            //右上角绘制启动等信息
            GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 200));
            if (GUILayout.Button("Start"))
            {
                NetServer.singleton.NetStart();
            }

            if (GUILayout.Button("Stop"))
            {
                NetServer.singleton.NetStop();
            }

            GUILayout.EndArea();
        }

        [Button]
        public void SendTest()
        {
            PacketHeader header = new PacketHeader();
            header.Id = TypeId<StringMessage>.ID;
            StringMessage stringMessage = new StringMessage();
            stringMessage.Msg = "Hello World";
            header.Body = stringMessage.ToByteString();
            NetServer.singleton.SendToAll(header);
        }
    }
}