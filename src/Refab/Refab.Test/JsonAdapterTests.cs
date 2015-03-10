using NUnit.Framework;
using StackExchange.Redis;

namespace Refab.Test
{
    [TestFixture]
    public class JsonAdapterTests
    {
        [Test]
        public void ShouldAdaptClass()
        {
            var tc = new TestClass {TestInteger = 8, TestString = "teststring"};
            var adapter = new JsonAdapter<TestClass>();
            RedisValue rv = adapter.Adapt(tc);
            Assert.IsNotNull(rv);
            string s = rv.ToString();
            TestClass b = adapter.Adapt(rv);
            Assert.AreEqual(tc.TestInteger, b.TestInteger);
            Assert.AreEqual(tc.TestString, b.TestString);
        }

        [Test]
        public void ShouldAdaptInteger()
        {
            int i = 5;
            var adapter = new JsonAdapter<int>();
            RedisValue rv = adapter.Adapt(i);
            Assert.IsNotNull(rv);
            string s = rv.ToString();
            int b = adapter.Adapt(rv);
            Assert.AreEqual(i, b);
        }
    }

    public class TestClass
    {
        public int TestInteger { get; set; }
        public string TestString { get; set; }
    }
}