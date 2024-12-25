namespace MyNUnit;

/// <summary>
/// Abstract record type to store MyNUnit test results
/// </summary>
/// <param name="Name"> Name of the test </param>
public abstract record TestResult(string Name);

/// <summary>
/// Record type to store MyNUnit ignored test results
/// </summary>
/// <param name="Name"> Name of the test </param>
public sealed record IgnoredTestResult(string Name, string Reason) : TestResult(Name);

/// <summary>
/// Record type to store MyNUnit successful test results
/// </summary>
/// <param name="Name"> Name of the test </param>
/// <param name="Time"> Time that test took to complete </param>
public sealed record SuccessfulTestResult(string Name, long Time) : TestResult(Name);

/// <summary>
/// Record type to store MyNUnit failed test results
/// </summary>
/// <param name="Name"> Name of the test </param>
/// <param name="Time"> Time that test took to complete </param>
/// <param name="Message"> Message of failed assertion </param>
public sealed record FailedTestResult(string Name, long Time, string? Message) : TestResult(Name);

/// <summary>
/// Record type to store MyNUnit failed test results
/// </summary>
/// <param name="Name"> Name of the test </param>
/// <param name="Time"> Time that test took to complete </param>
/// <param name="Ex"> Exception that interrupted test </param>
public sealed record FinishedWithExceptionResult(string Name, Exception Ex, long Time) : TestResult(Name);