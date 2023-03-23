using Unity.Netcode.Components;
using UnityEngine;

namespace Nico.Network
{
    /// <summary>
    /// 客户端权限的 NetworkTransform
    /// </summary>
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}