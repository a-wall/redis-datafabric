using System;
using System.Reactive.Linq;
using StackExchange.Redis;

namespace Refab
{
    public interface IDataFabric
    {
        IObservable<TValue> Observe<TValue, TKey>(TKey key);
        TValue Get<TValue, TKey>(TKey key);
        void Put<TValue, TKey>(TKey key, TValue value);
    }

    public class DataFabric : IDataFabric
    {
        private readonly IAdapterProvider<RedisValue> _valueAdapterProvider;
        private readonly TimeSpan? _expiryWhenPutting;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IAdapterProvider<RedisKey> _keyAdapterProvider;

        public DataFabric(ConnectionMultiplexer connectionMultiplexer, IAdapterProvider<RedisKey> keyAdapterProvider, IAdapterProvider<RedisValue> valueAdapterProvider, TimeSpan? expiryWhenPutting = null)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keyAdapterProvider = keyAdapterProvider;
            _valueAdapterProvider = valueAdapterProvider;
            _expiryWhenPutting = expiryWhenPutting;
        }

        public IObservable<TValue> Observe<TValue, TKey>(TKey key)
        {
            var keyAdapter = _keyAdapterProvider.Provide<TKey>();
            var rkey = keyAdapter.Adapt(key);
            var valueAdapter = _valueAdapterProvider.Provide<TValue>();
            return _connectionMultiplexer.GetObservable(rkey).Select(v => valueAdapter.Adapt(v)); ;
        }

        public TValue Get<TValue, TKey>(TKey key)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var keyAdapter = _keyAdapterProvider.Provide<TKey>();
            var rkey = keyAdapter.Adapt(key);
            var valueAdapter = _valueAdapterProvider.Provide<TValue>();
            var rvalue = database.StringGet(rkey);
            return valueAdapter.Adapt(rvalue);
        }

        public void Put<TValue, TKey>(TKey key, TValue value)
        {
            var keyAdapter = _keyAdapterProvider.Provide<TKey>();
            var rkey = keyAdapter.Adapt(key);
            var valueAdapter = _valueAdapterProvider.Provide<TValue>();
            var observer = _connectionMultiplexer.GetObserver(rkey, _expiryWhenPutting);
            observer.OnNext(valueAdapter.Adapt(value));
        }

    }
}