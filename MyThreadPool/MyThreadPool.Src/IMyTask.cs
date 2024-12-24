/*
MIT License

Copyright (c) 2024 Maxim Kamendov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace MyThreadPool;

/// <summary>
/// Interface that task should implement to be executed in MyThreadPool.
/// </summary>
/// <typeparam name="TResult">Type of the value that should be returned.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether task is completed.
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Gets the value calculated in task.
    /// If task is not completed, trying to get the value will cause block of the thread until value is calculated.
    /// </summary>
    public TResult Result { get; }

    /// <summary>
    /// Add a task to execute after this task and use it's value.
    /// </summary>
    /// <param name="func">Function that needs to be executed.</param>
    /// <typeparam name="TNewResult">Type of the return value of new task.</typeparam>
    /// <returns>New task.</returns>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}