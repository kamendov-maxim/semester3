namespace MyNUnit.Assertions;

[Serializable]
public class FailedAssertException : Exception
{
    public FailedAssertException() { }
    public FailedAssertException(string message) : base(message) { }
}