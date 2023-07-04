// using System;
// using System.Collections.Generic;
// using UnityEngine;
// namespace Nico
// {
//     public static class UGUIManager
//     {
//         private static Dictionary<Type, IUIPanel> _panels =new Dictionary<Type, IUIPanel>();
//
//         //编辑器模式下创建的UI 需要进行销毁
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//         public static void Init()
//         {
//             foreach (var kvp in _panels)
//             {
//                 kvp.Value.OnUnLoad();
//                 UnityEngine.Object.Destroy(kvp.Value.uiObj);
//             }
//             _panels.Clear();
//             
//             Application.quitting -= OnApplicationQuit;
//             Application.quitting += OnApplicationQuit;
//         }
//
//         private static void OnApplicationQuit()
//         {
//             foreach (var kvp in _panels)
//             {
//                 UnityEngine.Object.Destroy(kvp.Value.uiObj);
//             }
//             _panels.Clear();
//         }
//
//         public static void Register<T>(GameObject uiPrefab) where T : IUIPanel, new()
//         {
//             if (_panels.ContainsKey(typeof(T)))
//             {
//                 Debug.LogWarning($"UIPanel<{typeof(T)}> already exist");
//                 return;
//             }
//
//             // MonoBehaviour的UI
//             if (uiPrefab.TryGetComponent(out T monoPanel))
//             {
//                 monoPanel.uiObj = uiPrefab;
//                 monoPanel.OnInit();
//                 _panels.Add(typeof(T), monoPanel);
//                 return;
//             }
//
//             // 非MonoBehaviour的UI
//             var panel = new T
//             {
//                 uiObj = uiPrefab
//             };
//             panel.OnInit();
//             _panels.Add(typeof(T), panel);
//         }
//
//         public static void UnLoad<T>()
//         {
//             if (!_panels.TryGetValue(typeof(T), out IUIPanel panel)) return;
//             panel.OnUnLoad();
//             _panels.Remove(typeof(T));
//             UnityEngine.Object.Destroy(panel.uiObj);
//             return;
//         }
//
//         public static bool Open<T>() where T : IUIPanel
//         {
//             if (!_panels.TryGetValue(typeof(T), out IUIPanel panel)) return false;
//             panel.uiObj.SetActive(true);
//             panel.OnOpen();
//             return true;
//
//         }
//
//         public static bool Get<T>(out T panel) where T : IUIPanel
//         {
//             if (_panels.TryGetValue(typeof(T), out IUIPanel panel1))
//             {
//                 panel = (T)panel1;
//                 return true;
//             }
//
//             panel = default;
//             return false;
//         }
//
//         public static void Close<T>() where T : IUIPanel
//         {
//             if (!_panels.TryGetValue(typeof(T), out IUIPanel panel)) return;
//             panel.uiObj.SetActive(false);
//             panel.OnClose();
//         }
//     }
//
//
//     
// }