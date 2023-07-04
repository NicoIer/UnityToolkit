using NUnit.Framework;
using UnityEngine;

namespace Nico.Tests
{
    [TestFixture]
    public class ModelTest
    {
        public class TestModel01: IModel
        {
            public int Id;
            public void OnRegister()
            {
                Id = 123;
                // Debug.Log("Register");
                //此处可以从网络或者本地读取保存的数据
            }
            public void OnSave()
            {
                //此处可以保存数据到网络或者本地
                // Debug.Log("Save");
            }
        }
        [Test]
        public void TestModelExist()
        {
            ModelManager.Register<TestModel01>();
            var m1 =ModelManager.Get<TestModel01>();
            // Debug.Log($"m1:{m1}");
            Assert.IsTrue(m1.Id == 123);
        }
        
    }
}