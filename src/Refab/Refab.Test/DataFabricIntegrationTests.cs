using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using StackExchange.Redis;

namespace Refab.Test
{
    [TestFixture, Ignore("Integration test of DataFabric requires running local Redis instance")]
    public sealed class DataFabricIntegrationTests
    {
        private ConnectionMultiplexer _redis;
        private IAdapterProvider<RedisKey> _keyProvider;
        private IAdapterProvider<RedisValue> _valueProvider;

        [SetUp]
        public void SetUp()
        {
            _redis = ConnectionMultiplexer.Connect("localhost");
            _keyProvider = MockRepository.Mock<IAdapterProvider<RedisKey>>();
            var keyAdapter = MockRepository.Mock<IAdapter<string, RedisKey>>();
            keyAdapter.Stub(m => m.Adapt(string.Empty)).IgnoreArguments().Repeat.Any().DoInstead((Func<string,RedisKey>)(s => s));
            _keyProvider.Stub(m => m.Provide<string>()).Return(keyAdapter);
            _valueProvider = MockRepository.Mock<IAdapterProvider<RedisValue>>();
            var valueAdapter = MockRepository.Mock<IAdapter<string, RedisValue>>();
            valueAdapter.Stub(m => m.Adapt(string.Empty)).IgnoreArguments().Repeat.Any().DoInstead((Func<string, RedisValue>)(s => s));
            valueAdapter.Stub(m => m.Adapt((RedisValue)string.Empty)).IgnoreArguments().Repeat.Any().DoInstead((Func<RedisValue, string>)(s => s));
            _valueProvider.Stub(m => m.Provide<string>()).Return(valueAdapter);
        }

        [Test]
        public void ShouldPutString()
        {
            var v = "teststring";
            var k = "test-datafabric";

            var fabric = new DataFabric(_redis, _keyProvider, _valueProvider);


            fabric.Put(k, v);
            //var r = _redis.GetDatabase().StringGet(k);
            //StringAssert.AreEqualIgnoringCase(v, r.ToString());
            var o = fabric.Observe<string, string>(k);
            var s = o.FirstOrDefault();
            StringAssert.AreEqualIgnoringCase(v, s);
        }
    }
}
