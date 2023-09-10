using kcp2k;
using UnityEngine;

namespace Nico
{
    public class KcpGetter : MonoBehaviour, IServerTransportGetter, IClientTransportGetter
    {
        public KcpConfig config = KcpUtil.defaultConfig;
        public ushort port = 24419;

        public ClientTransport GetClient()
        {
            return new KcpClientTransport(config, port);
        }

        public ServerTransport GetServer()
        {
            return new KcpServerTransport(config, port);
        }
    }
}