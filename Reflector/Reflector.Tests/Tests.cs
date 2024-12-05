namespace Reflector.Tests;
using Reflector;

public class Tests
{
    [Test]
    public void Test1()
    {
        if (File.Exists("TestClass.cs"))
        {
            File.Delete("TestClass.cs");
        }
        ReflectorClass.PrintStructure(typeof(TestClass));
        if (!File.Exists("TestClass.cs"))
        {
            Assert.Fail("There is no TestClass.cs file created");
        }
        using FileStream expected = File.OpenRead("../../../PrintStructureAnswer.txt");
        using FileStream actual = File.OpenRead("TestClass.cs");
        FileAssert.AreEqual(expected, actual);
    }
}
