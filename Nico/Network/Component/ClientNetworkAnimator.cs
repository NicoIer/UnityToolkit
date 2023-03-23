using Unity.Netcode.Components;
using UnityEngine;

namespace Nico.Network
{
    /// <summary>
    /// 客户端权限的 NetworkAnimator
    /// </summary>
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}