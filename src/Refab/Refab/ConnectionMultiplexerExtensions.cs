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
        public static IObservable<RedisValue> GetObservable(this ConnectionMultiplexer connectionMultiplexer, RedisKey key, bool includeInitialValue = true)
        {
            var database = connectionMultiplexer.GetDatabase();
            var subscriber = connectionMultiplexer.GetSubscriber();
            var redisChannel = "__keyspace@0__:" + key.ToString();
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

        public static IObserver<RedisValue> GetObserver(this ConnectionMultiplexer connectionMultiplexer, RedisKey key, TimeSpan? expiry = null)
        {
            var database = connectionMultiplexer.GetDatabase();
            return Observer.Create<RedisValue>(s =>
            {
                database.StringSet(key, s, expiry);
            });
        }

        public static IEnumerable<string> GetKeys(this ConnectionMultiplexer connectionMultiplexer, string pattern)
        {
            var svrs = connectionMultiplexer.GetEndPoints();
            var keys = svrs.Select(ep => connectionMultiplexer.GetServer(ep)).Where(s => s.IsConnected && !s.IsSlave).SelectMany(s => s.Keys(pattern: pattern)).Select(k => k.ToString()).ToList();
            return keys;
        }
    }
}
