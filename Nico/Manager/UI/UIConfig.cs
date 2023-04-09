using System.Collections.Generic;
using UnityEngine;

namespace Nico.Manager
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Config/UIConfig", order = 0)]
    public class UIConfig : ScriptableObject, IConfig
    {
        public int defaultLoadCount = 5;
        public List<GameObject> canvasPrefabs = new();

        //要求GameObject上有Canvas组件 且 必须实现了 IUICanvas 接口
        //实现验证
        private void OnValidate()
        {
            //进行验证
            List<GameObject> usefulPrefabs = new List<GameObject>();
            foreach (var canvas in canvasPrefabs)
            {
                if (canvas == null)
                {
                    continue;
                }
                if (!canvas.TryGetComponent(out Canvas _))
                {
                    continue;
                }

                if (!canvas.TryGetComponent(out IUICanvas _))
                {
                    continue;
                }

                usefulPrefabs.Add(canvas);
            }

            canvasPrefabs = usefulPrefabs;
        }
    }
}