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
namespace MyThreadPool.Tests;

using System.Collections.Concurrent;
using System.Diagnostics;

public class Tests
{
    private readonly int _n = Environment.ProcessorCount;

    [Test]
    public void CheckThatThereAreNThreadsInPool()
    {
        var beforeThreadPoolInit = Process.GetCurrentProcess().Threads.Count;
        var threadPool = new MyThreadPool(_n);
        var afterThreadPoolInit = Process.GetCurrentProcess().Threads.Count;

        Assert.That(afterThreadPoolInit - beforeThreadPoolInit, Is.GreaterThanOrEqualTo(_n));
    }

    [Test]
    public void NumberOfTasksIsBiggerThanN()
    {
        var counter = 0;
        var threadPool = new MyThreadPool(_n);
        var taskNumber = _n + 5;
        var tasks = new IMyTask<int>[taskNumber];

        var someFunc = (int x) =>
        {
            Interlocked.Increment(ref counter);
            return ++x;
        };

        for (int i = 0; i < taskNumber; i++)
        {
            int x = i;
            tasks[i] = threadPool.Submit(() => someFunc(x));
        }

        for (int i = 0; i < taskNumber; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(++i));
        }

        Assert.That(counter, Is.EqualTo(taskNumber));
    }

    [Test]
    public void FunctionsThrowExceptions()
    {
        Func<int> someFunc = () => throw new IndexOutOfRangeException("Test exception");
        var threadPool = new MyThreadPool(_n);
        var tasks = new IMyTask<int>[_n];
        for (int i = 0; i < _n; i++)
        {
            tasks[i] = threadPool.Submit(someFunc);
        }

        foreach (var task in tasks)
        {
            Assert.Throws<AggregateException>(() => { var a = task.Result; });
        }
    }

    [Test]
    public void TestContinueWith()
    {
        var threadPool = new MyThreadPool(_n);
        var task1 = threadPool.Submit(() => { Thread.Sleep(100); return 1; });
        var task2 = task1.ContinueWith((int a) => a + 1);
        var task3 = task2
            .ContinueWith((a) => a + 1)
            .ContinueWith((a) => a + 1);
        var task4 = task1.ContinueWith((a) => a - 1);
        Assert.Multiple(() =>
        {
            Assert.That(task2.Result, Is.EqualTo(2));
            Assert.That(task3.Result, Is.EqualTo(4));
            Assert.That(task4.Result, Is.EqualTo(0));
        });
    }

    [Test]
    public void TryingToGetResultBeforeItIsCalculated()
    {
        var threadPool = new MyThreadPool(_n);
        var task = threadPool.Submit(() => { Thread.Sleep(20); return 5; });
        Assert.That(task.Result, Is.EqualTo(5));
    }

    [Test]
    public void ShutdownTest()
    {
        var threadPool = new MyThreadPool(_n);
        var tasks = new IMyTask<int>[_n];
        for (int i = 0; i < _n; i++)
        {
            int x = i;
            tasks[i] = threadPool.Submit(() => { Thread.Sleep(10); return x + 1; });
        }
        threadPool.Shutdown();
        for (int i = 0; i < _n; i++)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(i + 1));
        }
    }

    [Test]
    public void SubmittingAfterShutDownThrowsException()
    {
        var threadPool = new MyThreadPool(_n);
        threadPool.Shutdown();
        Assert.Throws<OperationCanceledException>(() =>
                threadPool.Submit(() => 0));
    }

    [Test]
    public void UsingThreadPoolFromMultipleThreads()
    {
        var threadPool = new MyThreadPool(_n);
        var list = new ConcurrentBag<IMyTask<int>>();
        var threads = new List<Thread>
        {
            new(() => list.Add(threadPool.Submit(() => 1))),
            new(() => list.Add(threadPool.Submit(() => 2))),
            new(() => list.Add(threadPool.Submit(() => 2)
                    .ContinueWith((a) => a + 1))),
            new(() => list.Add(threadPool.Submit(() => 2)
                    .ContinueWith((a) => a + 1)
                    .ContinueWith((a) => a + 1)))
        };
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());
        var answer = new int[4] { 1, 2, 3, 4 };
        var result = new int[4];
        int i = 0;
        foreach (var task in list)
        {
            var res = task.Result;
            result[i++] = task.Result;
        }
        Console.WriteLine();
        Array.Sort(result);
        Assert.That(result, Is.EqualTo(answer));
    }
}
