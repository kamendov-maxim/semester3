using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

using MyNUnit.Assertions;
using MyNUnit.Attributes;

namespace MyNUnit;

public class ClassTester
{
    public Type ClassToTest;
    private readonly ConcurrentBag<TestResult> _resultsList;
    public IEnumerable<MethodInfo?> BeforeClassMethods;
    public IEnumerable<MethodInfo?> AfterClassMethods;
    public IEnumerable<MethodInfo?> BeforeTestMethods;
    public IEnumerable<MethodInfo?> AfterTestMethods;
    public IEnumerable<MethodInfo?> TestMethods;
    private readonly object? _instance;

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