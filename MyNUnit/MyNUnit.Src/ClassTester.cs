using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

using MyNUnit.Assertions;
using MyNUnit.Attributes;

namespace MyNUnit;

/// <summary>
/// Class with methods to perform MyNUnit tests from some class 
/// </summary>
public class ClassTester
{
    /// <summary>
    /// Type of class to test
    /// </summary>
    public Type ClassToTest;
    private readonly ConcurrentBag<TestResult> _resultsList;

    /// <summary>
    /// Methods called before any tests from the class
    /// </summary>
    public IEnumerable<MethodInfo?> BeforeClassMethods;

    /// <summary>
    /// Methods called after all tests from the class
    /// </summary>
    public IEnumerable<MethodInfo?> AfterClassMethods;

    /// <summary>
    /// Methods called before performing any test
    /// </summary>
    public IEnumerable<MethodInfo?> BeforeTestMethods;

    /// <summary>
    /// Methods called after performing any test
    /// </summary>
    public IEnumerable<MethodInfo?> AfterTestMethods;

    /// <summary>
    /// Tests from class
    /// </summary>
    public IEnumerable<MethodInfo?> TestMethods;
    private readonly object? _instance;

    /// <summary>
    /// Constructor of class
    /// </summary>
    /// <param name="classToTest">Type of class to test</param>
    /// <param name="resultsList">List where to store results
    /// ConcurrentBag is used in case if tests will be performed in parallel</param>
    /// <returns></returns>
    public ClassTester(Type? classToTest, ConcurrentBag<TestResult> resultsList)
    {
        ArgumentNullException.ThrowIfNull(classToTest, "Test class type is null");
        ClassToTest = classToTest;
        BeforeClassMethods = ClassToTest.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(BeforeClassAttribute), false).Length > 0).ToArray();
        AfterClassMethods = ClassToTest.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(AfterClassAttribute), false).Length > 0).ToArray();
        BeforeTestMethods = ClassToTest.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(BeforeAttribute), false).Length > 0).ToArray();
        AfterTestMethods = ClassToTest.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(AfterAttribute), false).Length > 0).ToArray();
        TestMethods = ClassToTest.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0).ToArray();

        _resultsList = resultsList;

        if (classToTest == null)
            throw new NullReferenceException("Unable to create test class instance");
        _instance = Activator.CreateInstance(classToTest);
    }

    /// <summary>
    /// Run tests
    /// </summary>
    /// <returns>Task in which tests are running</returns>
    public Task Run()
    {
        return ClassToTest.IsDefined(typeof(ParallelTesting)) ? RunParallel() : RunSequential();
    }

    private Task RunSequential()
    {
        return Task.Run(() =>
        {
            Stopwatch st = new();

            foreach (var method in TestMethods)
            {
                ArgumentNullException.ThrowIfNull(method);

                TestAttribute attr = (TestAttribute)method.GetCustomAttributes(typeof(TestAttribute), true)[0];
                string? ignoreMessage = attr.IgnoreMessage;

                TestResult? testResult = null;

                if (ignoreMessage != null)
                {
                    testResult = new IgnoredTestResult(method.Name, ignoreMessage);
                    _resultsList.Add(testResult);
                    continue;
                }

                RunTestsInArray(BeforeTestMethods);

                st.Start();
                try
                {
                    var ret = method.Invoke(_instance, null);
                }
                catch (TargetInvocationException ex)
                // catch (FailedAssertException ex)
                {
                    st.Stop();
                    if (ex.InnerException == null)
                    {
                        throw new ArgumentNullException();
                    }
                    testResult = ex.InnerException.GetType() == typeof(FailedAssertException)
                        ? new FailedTestResult(method.Name, st.ElapsedMilliseconds, ex.InnerException.Message)
                        : new FinishedWithExceptionResult(method.Name, ex.InnerException, st.ElapsedMilliseconds);
                }
                if (testResult == null) { testResult = new SuccessfulTestResult(method.Name, st.ElapsedMilliseconds); }
                _resultsList.Add(testResult);
                RunTestsInArray(AfterTestMethods);
            }
            RunTestsInArray(AfterClassMethods);
        });
    }

    private Task RunParallel()
    {
        Stopwatch st = new();
        var tasks = new List<Task>();
        RunTestsInArrayParallel(BeforeClassMethods);
        return Task.Run(() => Parallel.ForEach(TestMethods, method =>
                    {
                        RunTestsInArrayParallel(BeforeTestMethods);

                        TestResult? testResult = null;

                        ArgumentNullException.ThrowIfNull(method);

                        TestAttribute attr = (TestAttribute)method.GetCustomAttributes(typeof(TestAttribute), true)[0];
                        string? ignoreMessage = attr.IgnoreMessage;
                        if (ignoreMessage != null)
                        {
                            testResult = new IgnoredTestResult(method.Name, ignoreMessage);
                            _resultsList.Add(testResult);
                        }
                        else
                        {

                            st.Start();
                            try
                            {
                                var ret = method.Invoke(_instance, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                st.Stop();
                                if (ex.InnerException == null)
                                {
                                    throw new ArgumentNullException();
                                }
                                testResult = ex.InnerException.GetType() == typeof(FailedAssertException)
                                    ? new FailedTestResult(method.Name, st.ElapsedMilliseconds, ex.InnerException.Message)
                                    : new FinishedWithExceptionResult(method.Name, ex.InnerException, st.ElapsedMilliseconds);
                            }
                            if (testResult == null) { testResult = new SuccessfulTestResult(method.Name, st.ElapsedMilliseconds); }
                            _resultsList.Add(testResult);
                            RunTestsInArrayParallel(AfterTestMethods);
                        }

                    })).ContinueWith(t => RunTestsInArrayParallel(AfterClassMethods));
    }

    private void RunTestsInArray(IEnumerable<MethodInfo?> array)
    {
        foreach (var method in array)
        {
            ArgumentNullException.ThrowIfNull(method, "Method is null");
            method.Invoke(_instance, null);
        }
    }

    private void RunTestsInArrayParallel(IEnumerable<MethodInfo?> array)
    {
        Parallel.ForEach(array, method =>
               {
                   ArgumentNullException.ThrowIfNull(method, "Method is null");
                   method.Invoke(_instance, null);
               });
    }
}