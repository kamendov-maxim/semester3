namespace MyNUnit;

public enum Result
{
    OK,
    Fail,
    Exception
}

public abstract record TestResult(string Name);

public sealed record IgnoredTestResult(string Name, string Reason) : TestResult(Name);
public sealed record SuccessfulTestResult(string Name, long Time) : TestResult(Name);
public sealed record FailedTestResult(string Name, long Time, string? Message) : TestResult(Name);
public sealed record FinishedWithExceptionResult(string Name, Exception Ex, long Time) : TestResult(Name);