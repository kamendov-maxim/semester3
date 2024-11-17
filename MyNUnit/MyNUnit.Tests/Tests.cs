using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

using MyNUnit;

namespace MyNUnit.Tests;

public class Tests
{
    private readonly string _pathToTests = "../../../TestDlls";
    private IEnumerable<TestResult> _results;
    private readonly int _successfullTests = 4;
    private readonly int _failedTests = 4;
    private readonly int _ignoredTests = 2;
    private readonly int _finishedWithExceptionTests = 2;


    [OneTimeSetUp]
    public void Setup()
    {

        var task = TestRunner.RunAsync(_pathToTests);
        task.Wait();
        _results = task.Result;
    }

    [Test]
    public void CheckSuccessfullTestResults()
    {
        IEnumerable<TestResult> res = _results.Where(r => r.GetType() == typeof(SuccessfulTestResult));
        Assert.That(res.Count, Is.EqualTo(_successfullTests));
        foreach (var r in res)
        {
            if (r.Name != "TestShouldPass" &&
                    r.Name != "ExceptionIsExpected")
            {
                Assert.Fail();
            }
        }
    }

    [Test]
    public void CheckFailedTestResults()
    {
        IEnumerable<FailedTestResult> res = _results.Where(r => r.GetType() == typeof(FailedTestResult)).Select(r => (FailedTestResult)r);
        Assert.That(res.Count, Is.EqualTo(_failedTests));
        foreach (var r in res)
        {
            if (r.Name != "TestShouldFail" &&
                    r.Name != "IncorrectException")
            {
                Assert.Fail();
            }
            Assert.That(r.Message, Is.EqualTo(
                        r.Name == "TestShouldFail" ? "Values are not equal" :
                        "Expected System.IO.InvalidDataException, catched System.SystemException"
                        ));
            // if (r.Name == "TestShouldFail")
            // {
            //     Assert.That(r.Message, Is.EqualTo("Values are not equal"));
            // }
            // else
            // {
            //     Assert.That(r.Message, Is.EqualTo("Expected System.IO.InvalidDataException, catched System.SystemException"));
            // }
        }
    }

    [Test]
    public void CheckFinishedWithExceptionTests()
    {
        IEnumerable<FinishedWithExceptionResult> res = _results.Where(r => r.GetType() == typeof(FinishedWithExceptionResult)).Select(r => (FinishedWithExceptionResult)r);
        Assert.That(res.Count, Is.EqualTo(_finishedWithExceptionTests));
        foreach (var r in res)
        {
            Assert.That(r.Name, Is.EqualTo("ExceptionIsNotExpected"));
        }
    }

    [Test]
    public void CheckIgnoredTests()
    {
        IEnumerable<IgnoredTestResult> res = _results.Where(r => r.GetType() == typeof(IgnoredTestResult)).Select(r => (IgnoredTestResult)r);
        Assert.That(res.Count, Is.EqualTo(_ignoredTests));
        foreach (var r in res)
        {
            Assert.That(r.Name, Is.EqualTo("IgnoredTest"));
            Assert.That(r.Reason, Is.EqualTo("This test should be ignored"));
        }
    }
}