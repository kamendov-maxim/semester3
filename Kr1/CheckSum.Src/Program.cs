using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

using CheckSum;

string helpMessage = "usage:\nCheckSumCalculator <path-to-file-or-directory>\n";

if (args.Length != 1)
{
    // Console.WriteLine(args.Length);
    Console.WriteLine("Incorrect input");
    Console.Write(helpMessage);
    return 0;
}

bool asyncModeEnabled = false;


try
{
    var stopwatch = new Stopwatch();
    var hash =
        asyncModeEnabled ?
        await CheckSumCalculator.CalculateAsync(args[1]) :
        CheckSumCalculator.Calculate(args[0]);

    stopwatch.Start();
    hash = CheckSumCalculator.Calculate(args[0]);
    stopwatch.Stop();
    Console.WriteLine($"Single-threaded\nResult: {string.Join(string.Empty, hash)}\n" +
            $"Time: {stopwatch.ElapsedMilliseconds} ms");
    stopwatch.Reset();
    stopwatch.Start();
    hash = await CheckSumCalculator.CalculateAsync(args[0]);
    stopwatch.Stop();
    Console.WriteLine($"Async\nResult: {string.Join(string.Empty, hash)}\n" +
            $"Time: {stopwatch.ElapsedMilliseconds} ms");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine("No such directory");
}
catch (FileNotFoundException)
{
    Console.WriteLine("No such file");
}

return 0;
