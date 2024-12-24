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
using System.Collections.Concurrent;

/// <summary>
/// ThreadPool implementation
/// </summary>
public class MyThreadPool : IDisposable
{
    private readonly CancellationTokenSource _tokenSource;
    private readonly ConcurrentQueue<Action> _queue;
    private readonly AutoResetEvent _newTask;
    private int _threadsFinished = 0;
    private readonly AutoResetEvent _newThreadFinished = new(true);
    private readonly MyThread[] _threads;

    /// <summary>
    /// Creates a thread pool instance <see cref="MyThreadPool"/>
    /// </summary>
    /// <param name="n">Amount of threads in thread pool</param>
    public MyThreadPool(int n)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, 100);
        _tokenSource = new();
        _newTask = new(false);
        _queue = new();
        _threads = new MyThread[n];
        for (int i = 0; i < n; i++)
        {
            _threads[i] = new(this);
        }
    }

    /// <summary>
    /// Shut down the thread pool. Already submitted tasks will be completed and new ones won't be submitted.
    /// </summary>
    public void Shutdown()
    {
        if (_tokenSource.IsCancellationRequested)
        {
            return;
        }

        _tokenSource.Cancel();

        while (_threadsFinished < _threads.Length)
        {
            _newThreadFinished.WaitOne();
            _newTask.Set();
        }
    }

    /// <summary>
    /// Add task to the queue. It will be started as soon as some thread
    /// </summary>
    /// <param name="func">Some function that has to be calculated</param>
    /// <exception cref="OperationCanceledException">Thrown if thread pool has already been shut down</exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (_tokenSource.IsCancellationRequested)
        {
            throw new OperationCanceledException("Thread pool has already been shut down");
        }
        var task = new MyTask<TResult>(this, func);
        _queue.Enqueue(task.Run);
        _newTask.Set();
        return task;
    }

    /// <summary>
    /// Shut the thread pool down before it is garbage collected
    /// </summary>
    public void Dispose() => Shutdown();

    private class MyThread
    {
        private readonly MyThreadPool _threadPool;
        private readonly Thread _thread;
        public void Join() => _thread.Join();

        public MyThread(MyThreadPool threadPool)
        {
            _threadPool = threadPool;
            _thread = new(EventLoop);
            _thread.Start();
        }

        public void EventLoop()
        {
            _threadPool._newTask.WaitOne();
            while (true)
            {
                if (_threadPool._queue.TryDequeue(out var task))
                {
                    task();
                    continue;
                }
                if (_threadPool._tokenSource.IsCancellationRequested)
                {
                    break;
                }
                _threadPool._newTask.WaitOne();
            }
            Interlocked.Increment(ref _threadPool._threadsFinished);
            _threadPool._newThreadFinished.Set();
            _thread.Join();
        }
    }

    private class MyTask<TResult>(MyThreadPool threadPool, Func<TResult> func) : IMyTask<TResult>
    {
        private readonly ManualResetEvent _completed = new(false);
        public bool IsCompleted { get; private set; } = false;
        private TResult? _result;
        private readonly Func<TResult> _func = func ?? throw new ArgumentNullException(nameof(func));
        private readonly MyThreadPool _threadPool = threadPool;
        private AggregateException? _exception;
        private readonly List<Action> _nextActions = [];

        public TResult Result
        {
            get
            {
                if (!IsCompleted)
                {
                    if (_threadPool._tokenSource.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Task wasn't completed before the thread pool shut down");
                    }
                    _completed.WaitOne();
                }
                if (_exception != null)
                {
                    throw _exception;
                }
                ArgumentNullException.ThrowIfNull(_result);
                return _result;
            }
        }

        public void Run()
        {
            try
            {
                _result = _func();
            }
            catch (Exception ex)
            {
                _exception = new AggregateException(ex);
            }
            finally
            {
                IsCompleted = true;
                _completed.Set();
            }
            foreach (var item in _nextActions)
            {
                _threadPool._queue.Enqueue(item);
                _threadPool._newTask.Set();
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
        {
            if (_threadPool._tokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException("Thread pool has already been shut down");
            }

            if (IsCompleted)
            {
                return _threadPool.Submit(() => func(Result));
            }
            var task = new MyTask<TNewResult>(_threadPool, () => func(Result));
            _nextActions.Add(task.Run);
            return task;
        }
    }
}