using System;
using StackExchange.Redis;

namespace Refab
{
    public sealed class JsonAdapterProvider : IAdapterProvider<RedisValue>
    {
        public IAdapter<T, RedisValue> Provide<T>()
        {
            return new JsonAdapter<T>();
        }
    }
}