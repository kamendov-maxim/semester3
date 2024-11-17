using System.Collections.Concurrent;
using System.Reflection;

namespace MyNUnit;

/// <summary>
/// Class with methods to extract and run tests from assemblies
/// </summary>
public class TestRunner
{
    /// <summary>
    /// Run tests in one or many assemblies
    /// </summary>
    /// <param name="pathToDlls">This can be the path to a separate assembly ending in .dll,
    /// or the path to the folder containing the assemblies. 
    /// In the latter case, the search for assemblies will be performed recursively in all subfolders
    /// </param>
    /// <returns></returns>
    public static async Task<IEnumerable<TestResult>> RunAsync(string pathToDll)
    {
        var results = new ConcurrentBag<TestResult>();

        if (pathToDll.EndsWith(".dll"))
        {
            await RunTestsInAssemblyAsync(pathToDll, results);
            return results;
        }
        var tasks = new List<Task>();
        var paths = Directory.GetFiles(pathToDll, "*.dll", SearchOption.AllDirectories);
        foreach (var path in paths)
        {
            tasks.Add(RunTestsInAssemblyAsync(path, results));
        }
        await Task.WhenAll(tasks);

        return results;
    }

    private static Task<ParallelLoopResult> RunTestsInAssemblyAsync(string pathToDll, ConcurrentBag<TestResult> results)
    {
        Assembly assembly = Assembly.LoadFrom(pathToDll);
        var classes = assembly.GetTypes();
        return Task.Run(() => Parallel.ForEach(classes,  c =>
        {
            var tester = new ClassTester(c, results);
            tester.Run().Wait();
        }));
    }
}