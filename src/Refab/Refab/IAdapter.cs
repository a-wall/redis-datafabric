using Newtonsoft.Json;
using StackExchange.Redis;

namespace Refab
{
    public interface IAdapter<T1, T2>
    {
        T1 Adapt(T2 value);
        T2 Adapt(T1 value);
    }

    public class JsonAdapter<T> : IAdapter<T, RedisValue>
    {
        public T Adapt(RedisValue value)
        {
            if (!value.HasValue) return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }

        public RedisValue Adapt(T value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}