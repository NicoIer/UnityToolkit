using NUnit.Framework;
using UnityEngine;
using UnityEngine.Assertions;
using Assert = NUnit.Framework.Assert;

namespace Nico.Tests
{
    [TestFixture]
    public class ObjectPoolTest
    {
        [Test]
        public void TestPrefabPool()
        {
            var obj = ObjectPoolManager.Get("Tree");
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.name == "Tree");
        }

        [Test]
        public void TestReturnPrefab()
        {
            var obj = new GameObject("123");
            ObjectPoolManager.Return(obj);
            Assert.IsTrue(obj.activeSelf == false);
            var o2 = ObjectPoolManager.Get("123");
            Assert.IsTrue(obj == o2);
        }
    }
}