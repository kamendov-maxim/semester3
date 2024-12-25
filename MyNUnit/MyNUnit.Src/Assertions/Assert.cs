namespace MyNUnit.Assertions;

public class Assert
{
    public static void That(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw message != null ? new FailedAssertException(message) : new FailedAssertException();
        }
    }

    public static void Expected<Ex>(Action testMethod) where Ex : Exception
    {
        try
        {
            testMethod();
        }
        catch (Exception catched)
        {
            if (catched.GetType() != typeof(Ex))
            {
                throw new FailedAssertException($"Expected {typeof(Ex)}, catched {catched.GetType()}");
            }
        }
    }
}