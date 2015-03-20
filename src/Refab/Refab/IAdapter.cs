using StackExchange.Redis;

namespace Refab
{
    public interface IAdapter<T1, T2>
    {
        T1 Adapt(T2 value);
        T2 Adapt(T1 value);
    }
}