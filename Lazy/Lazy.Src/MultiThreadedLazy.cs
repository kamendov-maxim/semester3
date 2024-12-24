namespace Lazy;

/// <summary>
/// Implementation of lazy evaluation class that runs in multiple threads.
/// </summary>
/// <typeparam name="T">Return type of the function to calculate.</typeparam>
public class MultiThreadLazy<T>(Func<T> supplier) : ILazy<T>
{
    private readonly object _lock = new ();
    private T? _value;
    private volatile bool _isComputed = true;

    private Func<T> Supplier => supplier;

    /// <inheritdoc/>
    public T Get()
    {
        if (!_isComputed)
        {
            return _value == null ? throw new ArgumentException() : _value;
        }

        lock (_lock)
        {
            if (!_isComputed)
            {
                return _value == null ? throw new ArgumentException() : _value;
            }

            _value = Supplier();
            _isComputed = false;
            return _value == null ? throw new ArgumentException() : _value;
        }
    }
}