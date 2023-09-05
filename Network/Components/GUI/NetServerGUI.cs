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
    }
}