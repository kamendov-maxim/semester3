namespace Lazy;

/// <summary>
/// An interface for realisation of class for lazy calculations.
/// </summary>
/// <typeparam name="T">Return value of function.</typeparam>
public interface ILazy<T>
{
    /// <summary>
    /// Get the value returned by function.
    /// If the function is already calculated, returns saved value.
    /// </summary>
    /// <returns>Result of function.</returns>
    T Get();
}