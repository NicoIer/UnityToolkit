using System;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>
    /// UI资源加载器
    /// </summary>
    public interface IUIComponentLoader
    {
        public IUIComponent Load();
    }
    
    // [Serializable]
    // public class ResourceUIComponentLoader:IUIComponentLoader
    // {
    //     public string path;
    //     public IUIComponent Load()
    //     {
    //         return Resources.Load<GameObject>(path).GetComponent<IUIComponent>();
    //     }
    // }
}