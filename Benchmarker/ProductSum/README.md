|             Method |  Count |        Mean |     Error |    StdDev | Ratio | RatioSD | Allocated |
|------------------- |------- |------------:|----------:|----------:|------:|--------:|----------:|
|     LinqProductSum |   1000 |    635.5 ns |   0.73 ns |   0.61 ns |  4.44 |    0.01 |         - |
|  ForLoopProductSum |   1000 |    207.0 ns |   4.13 ns |   5.51 ns |  1.42 |    0.04 |         - |
|    SSe2SProductSum |   1000 |    296.8 ns |   0.67 ns |   0.59 ns |  2.07 |    0.01 |         - |
| SpanLoopProductSum |   1000 |    143.2 ns |   0.34 ns |   0.28 ns |  1.00 |    0.00 |         - |
|                    |        |             |           |           |       |         |           |
|     LinqProductSum | 100000 | 64,087.8 ns | 325.15 ns | 288.24 ns |  4.02 |    0.02 |         - |
|  ForLoopProductSum | 100000 | 21,343.5 ns |  47.66 ns |  42.25 ns |  1.34 |    0.00 |         - |
|    SSe2SProductSum | 100000 | 31,477.3 ns | 145.72 ns | 136.31 ns |  1.97 |    0.01 |         - |
| SpanLoopProductSum | 100000 | 15,947.9 ns |  25.67 ns |  21.43 ns |  1.00 |    0.00 |         - |