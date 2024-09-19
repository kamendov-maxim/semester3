using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using MatrixMultiplication;

/// <summary>
/// Class to perform benchmarks on matrix multiplication
/// </summary>
public class Benchmark
{
    [Params(1, 5, 10, 100, 1000, 10000)]
    public int Size { get; set; }

    private readonly Matrix firstMatrix = new(0, 0);
    private readonly Matrix secondMatrix = new(0, 0);

    [GlobalSetup]
    public void GenerateMatrices()
    {
        var firstMatrix = new Matrix(Size, Size);
        var secondMatrix = new Matrix(Size, Size);
        var rnd = new Random();
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                firstMatrix[i, j] = rnd.Next();
                secondMatrix[i, j] = rnd.Next();
            }
        }
    }

    [Benchmark]
    public void WithoutMultiThreading() => MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix);
    public void WithMultiThreading() => MatrixMultiplier.Multiply(firstMatrix, secondMatrix);
}

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}
