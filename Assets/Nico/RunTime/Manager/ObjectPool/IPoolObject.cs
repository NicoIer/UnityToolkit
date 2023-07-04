namespace Nico
{
    public enum PoolObjectState
    {
        Spawned,
        Recycled
    }

    public interface IPoolObject
    {
        public PoolObjectState state { get; set; }
        void OnSpawn();
        void OnRecycle();
    }
}