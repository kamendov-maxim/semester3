namespace Lazy.Tests;

public class Tests
{
    private int _counter = 0;
    private readonly Random _rnd = new();

    [Test]
    public void TestIfReturnsSameValue()
    {
        ILazy<int> lazy = new SingleThreadLazy<int>(_rnd.Next);
        for (int i = 0; i < 2; i++)
        {
            var firstReturn = lazy.Get();
            var secondReturn = lazy.Get();
            Assert.That(firstReturn, Is.EqualTo(secondReturn));
            lazy = new MultiThreadLazy<int>(_rnd.Next);
        }
    }

    [Test]
    public void TestIfRunsOnlyOnce()
    {
        ILazy<int> lazy = new SingleThreadLazy<int>(() => { ++_counter; return 0; });
        for (int i = 0; i < 2; i++)
        {
            _counter = 0;
            lazy.Get();
            Assert.That(_counter, Is.EqualTo(1));
            lazy.Get();
            Assert.That(_counter, Is.EqualTo(1));
            lazy = new MultiThreadLazy<int>(() => { ++_counter; return 0; });
        }
    }

    [Test]
    public void MultiThreadTests()
    {
        var lazy = new MultiThreadLazy<int>(() => { ++_counter; return 0; });
        var threadNumber = Environment.ProcessorCount;
        var threads = new Thread[threadNumber];
        for (int i = 0; i < threadNumber; i++)
        {
            threads[i] = new Thread(() => lazy.Get());
        }
        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Assert.That(_counter, Is.EqualTo(1));
    }
}