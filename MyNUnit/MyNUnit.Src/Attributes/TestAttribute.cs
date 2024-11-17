namespace MyNUnit.Attributes;

/// <summary>
/// Attribute that tells MyNUnit that method is a test
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class TestAttribute(string? ignoreMessage = null) : Attribute
{
    public string? IgnoreMessage
    {
        get;
        set;
    } = ignoreMessage;
}

/// <summary>
/// An attribute indicating to my NUnit that this method should be among the methods
/// that will be called every time before running any test
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BeforeAttribute : Attribute
{

}

/// <summary>
/// An attribute indicating to my NUnit that this method should be among the methods
/// that will be called every time after running any test
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AfterAttribute : Attribute
{

}

/// <summary>
/// An attribute indicating to my NUnit that this method should be among the methods
/// that will be called every time after running all tests in the class
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AfterClassAttribute : Attribute
{

}

/// <summary>
/// An attribute indicating to my NUnit that this method should be among the methods
/// that will be called every time before running any tests in the class
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BeforeClassAttribute : Attribute
{

}