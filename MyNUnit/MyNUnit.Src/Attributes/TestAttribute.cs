namespace MyNUnit.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class TestAttribute(string? ignoreMessage = null) : Attribute
{
    public string? IgnoreMessage
    {
        get;
        set;
    } = ignoreMessage;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BeforeAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AfterAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AfterClassAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BeforeClassAttribute : Attribute
{

}