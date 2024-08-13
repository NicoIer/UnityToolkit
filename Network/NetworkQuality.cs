namespace Network
{
    public enum NetworkQuality : byte
    {
        Excellent, // 1-30ms
        Good, // 31-60ms
        Normal, // 61-100ms
        Poor, // 101-200ms
        Bad, // >200ms
    }
}