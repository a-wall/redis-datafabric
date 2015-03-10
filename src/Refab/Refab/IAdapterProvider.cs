namespace Refab
{
    public interface IAdapterProvider<T2>
    {
        IAdapter<T1, T2> Provide<T1>();
    }
}