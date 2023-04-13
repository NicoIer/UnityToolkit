using Nico.Manager;

namespace Nico.Data
{
    public class DataAspect
    {
        public readonly int id;

        public DataAspect(int id)
        {
            this.id = id;
        }

        public T GetMeta<T>() where T : IMetaData
        {
            return MetaManager.Instance.Get<T>(id);
        }

        public T GetContainer<T>() where T : ContainerData
        {
            return ContainerManager.Instance.Get<T>(id);
        }
    }
}