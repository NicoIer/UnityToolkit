using UnityToolkit;

namespace Network.Server
{
    public interface IServerSystem : ISystem, IOnInit<NetworkServer>
    {
    }
}