using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace Refab
{
    public class DataFabric
    {
        private readonly IAdapterProvider<RedisValue> _adapterProvider;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public DataFabric(ConnectionMultiplexer connectionMultiplexer, IAdapterProvider<RedisValue> adapterProvider)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _adapterProvider = adapterProvider;
        }

        public IObservable<T> Observe<T>(string key)
        {
            return _connectionMultiplexer.GetObservable<T>(_adapterProvider, key);
        }

        public void Put<T>(string key, T value)
        {
            IObserver<T> observer = _connectionMultiplexer.GetObserver<T>(_adapterProvider, key);
            observer.OnNext(value);
        }

        public IEnumerable<string> Keys(string pattern)
        {
            return _connectionMultiplexer.GetKeys(pattern);
        }
    }
}