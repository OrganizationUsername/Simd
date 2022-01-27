|           Method |  Count |        Mean |     Error |    StdDev | Ratio | RatioSD | Allocated |
|----------------- |------- |------------:|----------:|----------:|------:|--------:|----------:|
| ScalarProductSum |   1000 |    640.2 ns |   9.17 ns |   8.57 ns |  4.51 |    0.06 |         - |
| VectorProductSum |   1000 |    200.7 ns |   1.84 ns |   1.63 ns |  1.41 |    0.01 |         - |
|  SSe2SProductSum |   1000 |    296.9 ns |   1.33 ns |   1.24 ns |  2.09 |    0.01 |         - |
|   Avx2ProductSum |   1000 |    141.9 ns |   0.23 ns |   0.21 ns |  1.00 |    0.00 |         - |
|                  |        |             |           |           |       |         |           |
| ScalarProductSum | 100000 | 63,955.6 ns | 103.26 ns |  80.62 ns |  4.03 |    0.01 |         - |
| VectorProductSum | 100000 | 21,385.7 ns | 106.59 ns |  99.70 ns |  1.35 |    0.01 |         - |
|  SSe2SProductSum | 100000 | 31,446.3 ns | 189.85 ns | 177.58 ns |  1.98 |    0.01 |         - |
|   Avx2ProductSum | 100000 | 15,879.6 ns |  52.08 ns |  48.72 ns |  1.00 |    0.00 |         - |