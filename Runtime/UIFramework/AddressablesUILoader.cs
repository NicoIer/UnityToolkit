// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
//
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityToolkit;
//
// namespace UnityToolkit
// {
//     public class AddressablesUILoader : IUILoader
//     {
//         private Dictionary<int, GameObject> id2Asset = new Dictionary<int, GameObject>();
//
//         public GameObject Load<T>() where T : IUIPanel
//         {
//             var prefab = Addressables.LoadAssetAsync<GameObject>($"UI/{typeof(T).Name}.prefab").WaitForCompletion();
//             var hash = typeof(T).GetHashCode();
//             id2Asset.TryAdd(hash, prefab);
//             return prefab;
//         }
//
//         public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel
//         {
//             Addressables.LoadAssetAsync<GameObject>($"UI/{typeof(T).Name}.prefab").Completed += handle =>
//             {
//                 if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
//                 {
//                     callback?.Invoke(handle.Result);
//                     var hash = typeof(T).GetHashCode();
//                     id2Asset.TryAdd(hash, handle.Result);
//                 }
//                 else
//                 {
//                     Debug.LogError($"Failed to load UI panel: {typeof(T).Name}");
//                     callback?.Invoke(null);
//                 }
//             };
//         }
//
//         public Task<GameObject> LoadAsync<T>() where T : IUIPanel
//         {
//             var task = Addressables.LoadAssetAsync<GameObject>($"UI/{typeof(T).Name}.prefab").Task;
//
//             var hash = typeof(T).GetHashCode();
//             task.ContinueWith(t =>
//             {
//                 if (t.Status == TaskStatus.RanToCompletion)
//                 {
//                     id2Asset.TryAdd(hash, t.Result);
//                 }
//                 else
//                 {
//                     Debug.LogError($"Failed to load UI panel: {typeof(T).Name}");
//                 }
//             });
//
//             return task;
//         }
//
//         public void Dispose<T>(T panel) where T : IUIPanel
//         {
//             var hash = typeof(T).GetHashCode();
//             if (id2Asset.ContainsKey(hash))
//             {
//                 var prefab = id2Asset[hash];
//                 Addressables.Release(prefab);
//                 id2Asset.Remove(hash);
//             }
//         }
//     }
// }