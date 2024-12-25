namespace Lazy;

/// <summary>
/// Class representing lazy evaluation running in a single thread.
/// </summary>
/// <typeparam name="T">return parameter of function.</typeparam>
public class SingleThreadLazy<T>(Func<T> supplier) : ILazy<T>
{
    private T? _value;
    private bool _isComputed = true;

    private Func<T> Supplier => supplier;

    /// <inheritdoc/>
    public T Get()
    {
        if (_isComputed)
        {
            _value = Supplier();
        }

        _isComputed = false;
        return _value == null ? throw new ArgumentException() : _value;
    }
}