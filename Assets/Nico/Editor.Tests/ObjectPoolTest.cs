using NUnit.Framework;

namespace Nico.Tests
{
    [TestFixture]
    public class ObjectPoolTest
    {
        public class PoolObject1 : IPoolObject
        {
            public PoolObjectState state { get; set; }

            public PoolObject1()
            {
            }

            public void OnSpawn()
            {
            }

            public void OnRecycle()
            {
            }
        }

        [Test]
        public void TestPoolObject()
        {
            var obj = ObjectPoolManager.Get<PoolObject1>();
            Assert.IsTrue(obj.state==PoolObjectState.Spawned);
            ObjectPoolManager.Return(obj);
            Assert.IsFalse(obj.state==PoolObjectState.Spawned);
        }

        [Test]
        public void TestPrefabPool()
        {
            var obj = ObjectPoolManager.Get("PoolObject1");
            Assert.IsNull(obj);
            
        }
    }
}