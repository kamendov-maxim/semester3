public class SequencedTests
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
    public static void BeforeClassMethod()
    {

    }

    [MyNUnit.Attributes.Before]
    public void Before()
    {
    }

    [MyNUnit.Attributes.AfterClass]
    public static void AfterClassMethod()
    {

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

    [MyNUnit.Attributes.Test(IgnoreMessage = "This test should be ignored")]
    public void IgnoredTest()
    {
       throw new NotImplementedException();
    }
}