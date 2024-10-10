using System.Diagnostics;

using NUnit.Framework.Internal.Commands;
namespace MyThreadPool.Tests;

public class Tests
{
    public int Counter = 0;
    public int N = Environment.ProcessorCount;

    [Test]
    public void CheckThatThereAreNThreadsInPool()
    {
        var beforeThreadPoolInit = Process.GetCurrentProcess().Threads.Count;
        var threadPool = new MyThreadPool(N);
        var afterThreadPoolInit = Process.GetCurrentProcess().Threads.Count;

        Assert.That(afterThreadPoolInit - beforeThreadPoolInit, Is.GreaterThanOrEqualTo(N));
    }

    [Test]
    public void NumberOfTasksIsBiggerThanN()
    {
        var threadPool = new MyThreadPool(N);
        var taskNumber = N + 5;
        var tasks = new IMyTask<int>[taskNumber];
        var someFunc = (int x) =>
        {
            Interlocked.Increment(ref Counter);
            return ++x;
        };
        for (int i = 0; i < taskNumber; i++)
        {
            int x = i;
            tasks[i] = threadPool.Submit<int>(() => someFunc(x));
        }
        for (int i = 0; i < taskNumber; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(++i));
        }

        Assert.That(Counter, Is.EqualTo(taskNumber));
    }

    [Test]
    public void FunctionsThrowExceptions()
    {
        Func<int> someFunc = () => throw new IndexOutOfRangeException("Test exception");
        var threadPool = new MyThreadPool(N);
        var tasks = new IMyTask<int>[N];
        for (int i = 0; i < N; i++)
        {
            tasks[i] = threadPool.Submit<int>(someFunc);
        }

        foreach (var task in tasks)
        {
            Assert.Throws<AggregateException>(() => { var a = task.Result; });
        }
    }

    [Test]
    public void TestContinueWith()
    {
        var threadPool = new MyThreadPool(N);
        var task1 = threadPool.Submit(() => { return 1; });
        var task2 = task1.ContinueWith((int a) => a + 1);
        var task3 = task2
            .ContinueWith((a) => a + 1)
            .ContinueWith((a) => a + 1);
        Assert.That(task2.Result, Is.EqualTo(2));
        Assert.That(task3.Result, Is.EqualTo(4));
    }

    [Test]
    public void TryingToGetResultBeforeItIsCalculated()
    {
        var threadPool = new MyThreadPool(N);
        var task = threadPool.Submit(() => { Thread.Sleep(20); return 5; });
        Assert.That(task.Result, Is.EqualTo(5));
    }

    [Test]
    public void ShutdownTest()
    {
        var threadPool = new MyThreadPool(N);
        var tasks = new IMyTask<int>[N];
        for (int i = 0; i < N; i++)
        {
            int x = i;
            tasks[i] = threadPool.Submit(() => { Thread.Sleep(10); return x + 1; });
        }
        threadPool.Shutdown();
        for (int i = 0; i < N; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(i + 1));
        }
    }

    [Test]
    public void SubmittingAfterShutDownThrowsException()
    {
        var threadPool = new MyThreadPool(N);
        threadPool.Shutdown();
        Assert.Throws<OperationCanceledException>(() =>
                threadPool.Submit(() => { return 0; })
            );
    }
}
