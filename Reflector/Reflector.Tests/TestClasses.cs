namespace Reflector.Tests;

public class TestClass
{
    public string RetString(string str)
    {
        return "ABC";
    }

    public int RetInt()
    {
        return 10;
    }

    private bool PrivateMethod(int a, string b, bool c)
    {
        return true;
    }

    public static void StaticMethod(int a, int b)
    {

    }

    public static T GenericMethod<T>(T arg)
    {
        return arg;
    }
}

public class TestClass2
{
    public int RetInt()
    {
        return 10;
    }

    private bool PrivateMethod(int a, string b, bool c)
    {

        return true;
    }

    public static void StaticMethod(int a, int b)
    {

    }

    public static T GenericMethod<T>(T arg)
    {
        return arg;
    }
}
