using System;
using System.Collections.Generic;
using Nico.Design;
using UnityEngine;

namespace Nico.Manager
{
    public class UIManager : GlobalSingleton<UIManager>
    {
        //认为一个类型Canvas是唯一的 因此使用Dictionary<Type, IUICanvas>来存储
        [field: SerializeField] public UIConfig config { get; private set; }
        private Dictionary<Type, GameObject> _canvasMap; //用于存储 Type -> CanvasPrefab的映射
        private readonly Dictionary<Type, IUICanvas> canvasCache = new(); //用于存储Type -> CanvasGameObject的映射
        private Dictionary<Type, bool> _canvasShow = new();

        protected override void Awake()
        {
            base.Awake();
            // 加载预制体
            _canvasMap = new Dictionary<Type, GameObject>();
            _canvasShow = new Dictionary<Type, bool>();
            var count = 0;
            foreach (var canvas in config.canvasPrefabs)
            {
                canvas.TryGetComponent(out IUICanvas uiCanvas);
                var type = uiCanvas.GetType();
                _canvasMap.Add(type, canvas);
                _canvasShow.Add(type, false);
                
                //默认加载几个页面 避免第一次打开页面卡顿
                if (count >= config.defaultLoadCount) continue;
                LoadCanvas(type);
                ++count;
            }

        }

        public bool IsOpen<T>() where T : IUICanvas
        {
            return _canvasShow[typeof(T)];
        }

        public T GetCanvas<T>() where T : IUICanvas
        {
            if (!canvasCache.ContainsKey(typeof(T)))
            {
                LoadCanvas<T>();
            }

            var canvas = canvasCache[typeof(T)];
            return (T)canvas;
        }

        public void ShowCanvas<T>() where T : IUICanvas
        {
            if (!canvasCache.ContainsKey(typeof(T)))
            {
                LoadCanvas<T>();
            }

            var canvas = canvasCache[typeof(T)];

            _canvasShow[typeof(T)] = true;

            canvas.Show();
        }

        public void HideCanvas<T>() where T : IUICanvas
        {
            if (!canvasCache.ContainsKey(typeof(T)))
            {
                LoadCanvas<T>();
            }

            var canvas = canvasCache[typeof(T)];

            _canvasShow[typeof(T)] = false;

            canvas.Hide();
        }

        private void LoadCanvas(Type type)
        {
            if (canvasCache.ContainsKey(type))
            {
                return;
            }

            //否则实例化Canvas对象
            //获取预制体
            var objPrefab = _canvasMap[type];

            //实例化
            var obj = Instantiate(objPrefab, transform);
            var canvas = obj.GetComponent(type) as IUICanvas;
            canvasCache.Add(type, canvas);
            canvas.Init();
        }
        
        private void LoadCanvas<T>() where T : IUICanvas
        {
            LoadCanvas(typeof(T));
        }
    }
}