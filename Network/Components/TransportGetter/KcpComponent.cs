using kcp2k;
using UnityEngine;

namespace Nico
{
    public class KcpComponent : MonoBehaviour
    {
        public KcpConfig config = KcpUtil.defaultConfig;
        public ushort port = 24419;

        public KcpClientTransport GetClient()
        {
            return new KcpClientTransport(config, port);
        }

        public KcpServerTransport GetServer()
        {
            return new KcpServerTransport(config, port);
        }
    }
}