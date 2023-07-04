using System.Collections.Generic;
using Nico.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Nico.Tests
{
    [Parse]
    internal class MyTestParseClass
    {
        public int id;
        public string name;
    }

    internal enum ParseTestEnum01
    {
        X1,
        X2,
        X3
    }

    [TestFixture]
    public class ParseTests
    {
        [Test]
        public void ParseArrayExist()
        {
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(int[])));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(float[])));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(string[])));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(bool[])));
        }

        [Test]
        public void ParseListExist()
        {
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(System.Collections.Generic.List<int>)));
            Assert.AreEqual(true,
                ParserManager.Contains(typeof(string), typeof(System.Collections.Generic.List<float>)));
            Assert.AreEqual(true,
                ParserManager.Contains(typeof(string), typeof(System.Collections.Generic.List<string>)));
            Assert.AreEqual(true,
                ParserManager.Contains(typeof(string), typeof(System.Collections.Generic.List<bool>)));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(List<Vector2>)));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(List<Vector3>)));
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(List<Vector4>)));
        }


        [Test]
        public void ParseEnum()
        {
            ParserManager.Parse<string, ParseTestEnum01>("X1", out ParseTestEnum01 result);
            Assert.AreEqual(ParseTestEnum01.X1, result);
        }

        [Test]
        public void ParseInt()
        {
            ParserManager.Parse<string, int>("1231", out int result);
            Assert.AreEqual(1231, result);
            bool re = ParserManager.Parse<string, int>("1231-", out result);
            Assert.AreEqual(false, re);
        }

        [Test]
        public void ParseFloat()
        {
            ParserManager.Parse<string, float>("1231.1", out float result);
            Assert.AreEqual(1231.1f, result);
            bool re = ParserManager.Parse<string, float>("1231.1-", out result);
            Assert.AreEqual(false, re);
        }

        [Test]
        public void ParseString()
        {
            ParserManager.Parse<string, string>("1231", out string result);
            Assert.AreEqual("1231", result);
        }

        [Test]
        public void ParseBool()
        {
            ParserManager.Parse<string, bool>("true", out bool result);
            Assert.AreEqual(true, result);
            ParserManager.Parse<string, bool>("false", out result);
            Assert.AreEqual(false, result);
            bool re = ParserManager.Parse<string, bool>("1231-", out result);
            Assert.AreEqual(false, re);
        }

        [Test]
        public void ParseIntArray()
        {
            ParserManager.Parse<string, int[]>("1#2#3#4", out int[] result);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(4, result[3]);
            bool re = ParserManager.Parse<string, int[]>("1#2#3#4-", out result);
            Assert.AreEqual(false, re);
        }

        [Test]
        public void ParseFloatArray()
        {
            ParserManager.Parse<string, float[]>("1.1#2.2#3.3#4.4", out float[] result);
            Assert.AreEqual(1.1f, result[0]);
            Assert.AreEqual(2.2f, result[1]);
            Assert.AreEqual(3.3f, result[2]);
            Assert.AreEqual(4.4f, result[3]);
            bool re = ParserManager.Parse<string, float[]>("1.1#2.2#3.3#4.4-", out result);
            Assert.AreEqual(false, re);
        }

        [Test]
        public void ParseStringArray()
        {
            //string分割符是;
            ParserManager.Parse<string, string[]>("1#2#3#4", out string[] result);
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
            Assert.AreEqual("4", result[3]);
            bool re = ParserManager.Parse<string, string[]>("1#2#3#4-", out result);
            Assert.AreEqual("4-", result[3]);
            Assert.AreEqual(true, re);
        }


        [Test]
        public void TestParseBuildinInClass()
        {
            string str = "id=123;name=Test";
            BuildInStringGenericParser.StructAndClassParser(str, out MyTestParseClass result);
            Assert.AreEqual(123, result.id);
            Assert.AreEqual("Test", result.name);
        }

        [Test]
        public void TestParseClass()
        {
            Assert.AreEqual(true, ParserManager.Contains(typeof(string), typeof(MyTestParseClass)));
            string str = "id=123;name=Test";
            bool re = ParserManager.Parse<string, MyTestParseClass>(str, out MyTestParseClass result);
            Assert.AreEqual(true, re);
            Assert.AreEqual(123, result.id);
            Assert.AreEqual("Test", result.name);
        }
    }
}