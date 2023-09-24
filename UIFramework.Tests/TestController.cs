using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityToolkit;

namespace UIFramework.Test
{
    public class TestController: MonoBehaviour
    {
        [Button]
        public void Pop()
        {
            UIRoot.Singleton.CloseTop();
        }
        [Button]
        public void OpenPanel1()
        {
            UIRoot.Singleton.OpenPanel<TestPanel1>();
        }

        [Button]
        public void OpenPanel2()
        {
            UIRoot.Singleton.OpenPanel<TestPanel2>();
        }
        
        [Button]
        public void ClosePanel1()
        {
            UIRoot.Singleton.ClosePanel<TestPanel1>();
        }
        
        [Button]
        public void ClosePanel2()
        {
            UIRoot.Singleton.ClosePanel<TestPanel2>();
        }
        
        [Button]
        public void CloseAllPanel()
        {
            UIRoot.Singleton.CloseAll();
        }
        
        [Button]
        public void DisposePanel1()
        {
            UIRoot.Singleton.Dispose<TestPanel1>();
        }
        
        [Button]
        public void DisposePanel2()
        {
            UIRoot.Singleton.Dispose<TestPanel2>();
        }
    }
}