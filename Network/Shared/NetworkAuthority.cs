namespace Network
{
    // public enum NetworkAuthority : byte
    // {
    //     ServerOnly,
    //     ClientOnly,
    //     Both
    // }

    public enum SyncMode
    {
        Observers,
        Owner
    }

    public enum SyncDirection
    {
        ServerToClient,
        ClientToServer
    }
}