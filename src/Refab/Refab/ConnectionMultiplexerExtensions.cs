using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using StackExchange.Redis;

namespace Refab
{
    public static class ConnectionMultiplexerExtensions
    {
        public static IObservable<RedisValue> GetObservable(this ConnectionMultiplexer connectionMultiplexer, string key, bool includeInitialValue = true)
        {
            var database = connectionMultiplexer.GetDatabase();
            var subscriber = connectionMultiplexer.GetSubscriber();
            var redisChannel = "__keyspace@0__:" + key;
            var getFunc = new Func<RedisValue>(() => database.StringGet(key));
            return Observable.Create<RedisValue>(observer =>
            {
                subscriber.Subscribe(redisChannel, (channel, value) =>
                {
                    if ((string)value == "set")
                    {
                        observer.OnNext(getFunc());
                    }
                });
                if (includeInitialValue)
                {
                    observer.OnNext(getFunc());
                }
                return Disposable.Create(() =>
                {
                    subscriber.Unsubscribe(redisChannel);
                });
            }).Where(v => v.HasValue);
        }

        public static IObservable<T> GetObservable<T>(this ConnectionMultiplexer connectionMultiplexer, IAdapterProvider<RedisValue> adapterProvider, string key,
            bool includeInitialValue = true)
        {
            var adapter = adapterProvider.Provide<T>();
            return connectionMultiplexer.GetObservable(key, includeInitialValue).Select(v => adapter.Adapt(v));
        }

        public static IObserver<RedisValue> GetObserver(this ConnectionMultiplexer connectionMultiplexer, string key)
        {
            var database = connectionMultiplexer.GetDatabase();
            return Observer.Create<RedisValue>(s =>
            {
                database.StringSet(key, s);
            });
        }

        public static IObserver<T> GetObserver<T>(this ConnectionMultiplexer connectionMultiplexer, IAdapterProvider<RedisValue> adapterProvider, string key)
        {
            var adapter = adapterProvider.Provide<T>();
            var observer = connectionMultiplexer.GetObserver(key);
            return Observer.Create<T>(t => observer.OnNext(adapter.Adapt(t)));
        }

        public static IEnumerable<string> GetKeys(this ConnectionMultiplexer connectionMultiplexer, string pattern)
        {
            var svrs = connectionMultiplexer.GetEndPoints();
            var keys = svrs.Select(ep => connectionMultiplexer.GetServer(ep)).Where(s => s.IsConnected && !s.IsSlave).SelectMany(s => s.Keys(pattern: pattern)).Select(k => k.ToString()).ToList();
            return keys;
        }
    }
}
