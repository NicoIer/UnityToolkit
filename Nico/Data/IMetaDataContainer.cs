namespace Nico.Util
{
    public interface IMetaDataContainer
    {
        public void AddData(IMetaData metaData);
        public IMetaData GetMetaData(int idx);
    }
}