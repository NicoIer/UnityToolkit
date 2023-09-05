namespace Nico
{
    public interface INetTransport
    {
        public int GetMaxPacketSize(int channelId = Channels.Reliable);
        public int GetBatchThreshold(int channelId = Channels.Reliable) => GetMaxPacketSize(channelId);
        
        public void Shutdown();
    }
}