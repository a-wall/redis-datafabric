using System;
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

        public static IObserver<RedisValue> GetObserver(this ConnectionMultiplexer connectionMultiplexer, string key)
        {
            var database = connectionMultiplexer.GetDatabase();
            return Observer.Create<RedisValue>(s =>
            {
                database.StringSet(key, s);
            });
        }
    }
}
