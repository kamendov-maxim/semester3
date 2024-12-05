using System.Reflection;
using System.Text;

namespace Reflector;

/// <summary>
/// Reflector class to print or compare classes
/// </summary>
public class ReflectorClass
{
    /// <summary>
    /// Print class in <class-name>.cs file
    /// </summary>
    /// <param name="someClass">Class to print</param>
    public static void PrintStructure(Type someClass)
    {
        using StreamWriter outputFile = new StreamWriter(someClass.Name.Split('.').Last() + ".cs");
        {
            outputFile.WriteLine($"public class {someClass.Name.Split('.').Last()}");
            outputFile.WriteLine("{");
            foreach (var method in someClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                        | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                outputFile.WriteLine(ReadMethod(method));

            }
            outputFile.WriteLine("}");
        }
    }

    private static string ReadMethod(MethodInfo method)
    {
        var builder = new StringBuilder("    ");
        if (method.IsPublic)
            builder.Append("public ");
        else if (method.IsPrivate)
            builder.Append("private ");
        else if (method.IsAssembly)
            builder.Append("internal ");
        if (method.IsFamily)
            builder.Append("protected ");
        if (method.IsStatic)
            builder.Append("static ");
        builder.Append(method.ReturnType);
        builder.Append(' ');
        builder.Append(method.Name);

        bool firstArg = true;
        if (method.IsGenericMethod)
        {
            builder.Append('<');
            foreach (var g in method.GetGenericArguments())
            {
                if (firstArg)
                    firstArg = false;
                else
                    builder.Append(", ");
                builder.Append(g);
            }
            builder.Append('>');
        }

        builder.Append('(');

        firstArg = true;
        foreach (var param in method.GetParameters())
        {
            if (!firstArg)
            {
                builder.Append(", ");
            }
            if (param.ParameterType.IsByRef)
                builder.Append("ref ");
            else if (param.IsOut)
                builder.Append("out ");
            firstArg = false;
            builder.Append(param.ParameterType);
            builder.Append(' ');
            builder.Append(param.Name);
        }
        builder.Append(')');
        builder.Append("\n    {\n    }\n");
        return builder.ToString();
    }

    /// <summary>
    /// Print methods that appear in one class and do not in other and vice versa
    /// </summary>
    /// <param name="a">First class</param>
    /// <param name="b">Second class</param>
    public static void DiffClasses(Type a, Type b)
    {
        var methodsOfA = a.GetMethods().Select(m => ReadMethod(m));
        var methodsOfB = b.GetMethods().Select(m => ReadMethod(m));
        var firstNotSecond = methodsOfA.Except(methodsOfB).ToList();
        var secondNotFirst = methodsOfB.Except(methodsOfA).ToList();
        if (firstNotSecond.Count > 0)
        {
            Console.WriteLine("Exists in first class but not in second:");
            foreach (var entry in firstNotSecond)
            {
                Console.WriteLine(entry);
            }

        }
        if (secondNotFirst.Count > 0)
        {
            Console.WriteLine("Exists in second class but not in first:");
            foreach (var entry in secondNotFirst)
            {
                Console.WriteLine(entry);
            }
        }
    }
}
