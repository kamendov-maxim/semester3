namespace Lazy;

public interface ILazy<T> { T Get(); }

public class SingleThreadLazy<T>(Func<T> supplier) : ILazy<T>
{
    private readonly T[] _value = new T[1];
    private bool _firstTime = true;
    private Func<T> Supplier => supplier;

    public T Get()
    {
        if (_firstTime)
        {
            _value[0] = Supplier();
        }
        _firstTime = false;
        return _value[0];
    }
}

public class MultiThreadLazy<T>(Func<T> supplier) : ILazy<T>
{
    private readonly T[] _value = new T[1];
    private bool _firstTime = true;
    private readonly object _lock = new();
    private Func<T> Supplier => supplier;

    public T Get()
    {
        if (!_firstTime)
        {
            return _value[0];
        }
        lock (_lock)
        {
            _value[0] = Supplier();
            _firstTime = false;
            return _value[0];
        }
    }
}
