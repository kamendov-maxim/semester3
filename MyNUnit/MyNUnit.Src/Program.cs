﻿using MyNUnit;

if (args.Length != 1)
{
    if (args.Length > 0 && (args[0] != "-h" || args[0] != "--help"))
        Console.WriteLine("Incorrect input");
    Console.WriteLine("Usage:\nMyNUnit <path-to-dll-with-tests> - run tests located at the given path");
    return 1;
}


IEnumerable<TestResult> results;
try
{
    results = await TestRunner.RunAsync(args[0]);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
    return 0;
}

foreach (var item in results.Where(r => r.GetType() == typeof(FailedTestResult)))
{
    Console.WriteLine($"item {item.Name}");
}

var failed = 0;
var finishedWithException = 0;
var ignored = 0;
foreach (var result in results)
{
    switch (result)
    {
        case IgnoredTestResult ignoredTestResult:
            {
                Console.WriteLine($"\x1b[97mTest {ignoredTestResult.Name} was ignored");
                Console.WriteLine($"Message: {ignoredTestResult.Reason}");
                ++ignored;
                break;
            }
        case SuccessfulTestResult:
            {
                Console.WriteLine($"\x1b[92mTest {result.Name} passed");
                break;
            }
        case FailedTestResult failedTestResult:
            {
                Console.WriteLine($"\x1b[91mTest {failedTestResult.Name} failed");
                if (failedTestResult.Message != null)
                {
                    Console.WriteLine($"Assertion message: {failedTestResult.Message}");
                }
                Console.Write("\x1b[39m");
                ++failed;
                break;
            }
        case FinishedWithExceptionResult exceptionResult:
            {
                Console.WriteLine($"\x1b[93mTest {exceptionResult.Name} finished with exception {exceptionResult.Ex.GetType()}");
                if (exceptionResult.Ex.Message != null)
                {
                    Console.WriteLine($"Message: {exceptionResult.Ex.Message}");
                }
                ++finishedWithException;
                break;
            }

    }
    Console.Write("\x1b[39m");
}

Console.WriteLine($"\x1b[1m{results.Count()} tests completed, {failed} failed, " +
        $"{results.Count() - failed - finishedWithException - ignored} passed, {finishedWithException} finished with exception, {ignored} ignored\x1b[22m");
return 0;