# Benchmarks

``` ini
BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.2.402
  [Host]     : .NET Core 2.2.7 (CoreCLR 4.6.28008.02, CoreFX 4.6.28008.03), 64bit RyuJIT DEBUG
  DefaultJob : .NET Core 2.2.7 (CoreCLR 4.6.28008.02, CoreFX 4.6.28008.03), 64bit RyuJIT
```

|         Method |     Mean |    Error |   StdDev |
|--------------- |---------:|---------:|---------:|
|          Small | 162.7 ns | 2.023 ns | 1.893 ns |
| SmallBuffer100 | 146.2 ns | 2.068 ns | 1.934 ns |

|         Method |     Mean |    Error |   StdDev |
|--------------- |---------:|---------:|---------:|
|          Large | 217.0 ns | 2.138 ns | 1.999 ns |
| LargeBuffer100 | 184.7 ns | 1.609 ns | 1.505 ns |
