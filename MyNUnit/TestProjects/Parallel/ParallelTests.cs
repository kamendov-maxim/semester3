using MyNUnit;
using MyNUnit;

[MyNUnit.Attributes.ParallelTesting]
public class ParallelTests
{
    private readonly int a = 10;
    private readonly int b = 10;
    private readonly int c = 5;
    private int counter = 0;

    private void FunctionThatThrowsException()
    {
        throw new SystemException();
    }


    [MyNUnit.Attributes.BeforeClass]
    public static void BeforeClass()
    {

    }

    [MyNUnit.Attributes.AfterClass]
    public static void AfterClass()
    {

    }

    [MyNUnit.Attributes.Before]
    public void Before()
    {

    }

    [MyNUnit.Attributes.After]
    public void After()
    {
        Interlocked.Increment(ref counter);
    }

    [MyNUnit.Attributes.Test]
    public void TestShouldPass()
    {
        MyNUnit.Assertions.Assert.That(a == b);
    }

    [MyNUnit.Attributes.Test]
    public void TestShouldFail()
    {
        MyNUnit.Assertions.Assert.That(a == c, "Values are not equal");
    }

    [MyNUnit.Attributes.Test]
    public void ExceptionIsNotExpected()
    {
        FunctionThatThrowsException();
    }

    [MyNUnit.Attributes.Test]
    public void ExceptionIsExpected()
    {
        MyNUnit.Assertions.Assert.Expected<SystemException>(FunctionThatThrowsException);
    }

    [MyNUnit.Attributes.Test]
    public void IncorrectException()
    {
        MyNUnit.Assertions.Assert.Expected<InvalidDataException>(FunctionThatThrowsException);
    }

    [MyNUnit.Attributes.Test("This test should be ignored")]
    public void IgnoredTest()
    {
       throw new NotImplementedException();
    }

}