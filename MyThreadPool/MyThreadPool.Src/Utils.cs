namespace MyThreadPool;

internal class MyThread
{
    private readonly MyThreadPool _threadPool;
    private readonly Thread _thread;
    public bool Waiting = true;
    public void Join() => _thread.Join();

    public MyThread(MyThreadPool threadPool)
    {
        _threadPool = threadPool;
        _thread = new(EventLoop);
        _thread.Start();
    }

    public void EventLoop()
    {
        _threadPool.NewTask.WaitOne();
        while (true)
        {
            if (_threadPool.Queue.TryDequeue(out var task))
            {
                Waiting = false;
                task();
                continue;
            }
            if (_threadPool.TokenSource.IsCancellationRequested)
            {
                break;
            }
            Waiting = true;
            _threadPool.NewTask.WaitOne();
        }
        Interlocked.Increment(ref _threadPool.ThreadsFinished);
        _threadPool.NewThreadFinished.Set();
        _thread.Join();
    }
}

internal class MyTask<TResult>(MyThreadPool threadPool, Func<TResult> func) : IMyTask<TResult>
{
    private readonly ManualResetEvent _completed = new(false);
    public bool IsCompleted { get; private set; } = false;
    private TResult? _result;
    private readonly Func<TResult> _func = func ?? throw new ArgumentNullException("Function cannot be null");
    private MyThreadPool ThreadPool { get; } = threadPool;
    private AggregateException? _exception;
    private Action? _next;

    public TResult Result
    {
        get
        {
            if (IsCompleted)
            {
                if (_exception != null)
                {
                    throw _exception;
                }
                return _result!;
            }
            _completed.WaitOne();
            return _result!;
        }
    }

    public void Run()
    {
        try
        {
            _result = _func();
            _completed.Set();
        }
        catch (Exception ex)
        {
            _exception = new AggregateException(ex);
        }
        finally
        {
            IsCompleted = true;
        }
        _next?.Invoke();
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
    {
        if (ThreadPool.TokenSource.IsCancellationRequested)
        {
            throw new OperationCanceledException("Thread pool has already been shut down");
        }

        if (IsCompleted)
        {
            return ThreadPool.Submit<TNewResult>(() => func(Result));
        }
        var task = new MyTask<TNewResult>(ThreadPool, () => func(Result));
        _next = task.Run;
        return task;
    }
}
