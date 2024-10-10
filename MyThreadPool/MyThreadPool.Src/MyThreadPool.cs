using System.Collections.Concurrent;
namespace MyThreadPool;

/// <summary>
/// ThreadPool implementation
/// </summary>
public class MyThreadPool : IDisposable
{
    internal readonly CancellationTokenSource TokenSource;
    internal readonly ConcurrentQueue<Action> Queue;
    internal readonly AutoResetEvent NewTask;
    internal  int ThreadsFinished = 0;
    internal AutoResetEvent NewThreadFinished = new(true);
    private readonly MyThread[] _threads;

    /// <summary>
    /// Creates a thread pool instance <see cref="MyThreadPool"/>
    /// </summary>
    /// <param name="n">Amount of threads in thread pool</param>
    public MyThreadPool(int n)
    {
        TokenSource = new();
        NewTask = new(false);
        Queue = new();
        _threads = new MyThread[n];
        for (int i = 0; i < n; i++)
        {
            _threads[i] = new(this);
        }
    }

    /// <summary>
    /// Shut down the thread pool. Already submitted tasks will be completed and new ones won't be submitted
    /// </summary
    public void Shutdown()
    {
        if (TokenSource.IsCancellationRequested)
        {
            return;
        }

        TokenSource.Cancel();

        while (ThreadsFinished < _threads.Length)
        {
            NewThreadFinished.WaitOne();
            NewTask.Set();
        }
    }

    /// <summary>
    /// Add task to the queue. It will be started as soon as some thread
    /// </summary>
    /// <param name="func">Some function that has to be calculated</param>
    /// <exception cref="OperationCanceledException">Thrown if thread pool has already been shut down</exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (TokenSource.IsCancellationRequested)
        {
            throw new OperationCanceledException("Thread pool has already been shut down");
        }

        var task = new MyTask<TResult>(this, func);
        Queue.Enqueue(task.Run);
        NewTask.Set();
        return task;
    }

    /// <summary>
    /// Shut the thread pool down before it is garbage collected
    /// </summary>
    public void Dispose()
    {
        Shutdown();
    }
}