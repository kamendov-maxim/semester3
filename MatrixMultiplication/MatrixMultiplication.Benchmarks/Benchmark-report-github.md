```

BenchmarkDotNet v0.14.0, Arch Linux
Intel Core i5-8257U CPU 1.40GHz (Coffee Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.401
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                | Size  | Mean     | Error    | StdDev   |
|---------------------- |------ |---------:|---------:|---------:|
| **WithoutMultiThreading** | **1**     | **64.57 ns** | **0.817 ns** | **0.765 ns** |
| **WithoutMultiThreading** | **5**     | **65.02 ns** | **0.484 ns** | **0.429 ns** |
| **WithoutMultiThreading** | **10**    | **65.23 ns** | **1.130 ns** | **1.057 ns** |
| **WithoutMultiThreading** | **100**   | **61.13 ns** | **0.329 ns** | **0.257 ns** |
| **WithoutMultiThreading** | **1000**  | **63.71 ns** | **0.116 ns** | **0.097 ns** |
| **WithoutMultiThreading** | **10000** | **61.99 ns** | **0.577 ns** | **0.511 ns** |
