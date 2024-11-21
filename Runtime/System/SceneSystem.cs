#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UnityToolkit
{
    
  
    public abstract class SceneContext : ScriptableObject,IOnInit,IDisposable
    {
        public abstract void OnActiveSceneChanged(Scene prev, Scene now);
        public abstract void OnInit();
        public abstract void Dispose();
    }
    
    public class SceneSystem : MonoBehaviour, IOnInit, ISystem
    {
        [SerializeField]
        private List<SceneContext> sceneContexts = new List<SceneContext>();
        public void OnInit()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            foreach (var context in sceneContexts)
            {
                context.OnInit();
            }
        }
        

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            foreach (var context in sceneContexts)
            {
                context.Dispose();
            }
        }

        private void OnActiveSceneChanged(Scene prev, Scene now)
        {
            foreach (var context in sceneContexts)
            {
                context.OnActiveSceneChanged(prev, now);
            }
        }
    }
}
#endif
