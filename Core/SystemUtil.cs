using System;
using System.Net.NetworkInformation;

namespace UnityToolkit
{
    public static class SystemUtil
    {
        public static string MacAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return adapter.GetPhysicalAddress().ToString();
                }
            }
            return string.Empty;
        }
    }    
}
