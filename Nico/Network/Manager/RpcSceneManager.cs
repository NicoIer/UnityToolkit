using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Nico.Network
{
    /// <summary>
    /// 基于Rpc的场景同步管理器
    /// 会通知所有客户端进行场景加载
    /// </summary>
    // public class RpcSceneManager : GlobalNetSingleton<RpcSceneManager>
    // {
    //     public event Action OnLoadSceneCompleted;
    //     private Dictionary<ulong, bool> _clientsLoadDic = new Dictionary<ulong, bool>();
    //
    //     public void LoadScene(string sceneName)
    //     {
    //         _clientsLoadDic.Clear();
    //         //用当前所有连接的客户端ID和false填充字典
    //         foreach (var clientID in NetworkManager.Singleton.ConnectedClientsIds)
    //         {
    //             _clientsLoadDic.Add(clientID, false);
    //         }
    //
    //         _LoadSceneClientRpc(sceneName);
    //     }
    //
    //     [ServerRpc(RequireOwnership = false)]
    //     private void _LoadCompleteServerRpc(ServerRpcParams serverRpcParams = default)
    //     {
    //         _clientsLoadDic[serverRpcParams.Receive.SenderClientId] = true;
    //         //如果所有客户端都加载完毕
    //         if (_CheckAllClientsLoaded())
    //         {
    //             OnLoadSceneCompleted?.Invoke();
    //         }
    //     }
    //
    //     [ClientRpc]
    //     private void _LoadSceneClientRpc(string sceneName)
    //     {
    //         //客户端开始加载场景
    //         SceneManager.LoadScene(sceneName);
    //         //加载完成后调用LoadCompleteClientRpc 通知服务器我已加载完毕
    //         _LoadCompleteServerRpc();
    //     }
    //
    //     private bool _CheckAllClientsLoaded()
    //     {
    //         foreach (var isLoaded in _clientsLoadDic.Values)
    //         {
    //             if (!isLoaded)
    //             {
    //                 return false;
    //             }
    //         }
    //
    //         return true;
    //     }
    // }
}