Current:
|                        Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |  Gen 0 | Allocated |
|------------------------------ |------------:|----------:|----------:|-------:|--------:|-------:|----------:|
|      ScalarFilterAllWordsForS | 78,833.7 ns | 791.76 ns | 740.61 ns | 764.27 |   11.17 |      - |     104 B |
| SimdFilterAllWordsForSForeach |    103.2 ns |   1.29 ns |   1.21 ns |   1.00 |    0.00 | 0.0167 |     280 B |

Previous:
|                        Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |  Gen 0 | Allocated |
|------------------------------ |------------:|----------:|----------:|-------:|--------:|-------:|----------:|
|      ScalarFilterAllWordsForS | 74,823.2 ns | 926.04 ns | 866.22 ns | 721.62 |   10.62 |      - |     104 B |
| SimdFilterAllWordsForSForeach |    103.6 ns |   0.67 ns |   0.59 ns |   1.00 |    0.00 | 0.0167 |     280 B |

Previous:
|                        Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |  Gen 0 | Allocated |
|------------------------------ |------------:|----------:|----------:|-------:|--------:|-------:|----------:|
|      ScalarFilterAllWordsForS | 71,758.8 ns | 474.82 ns | 444.15 ns | 562.84 |   15.48 |      - |     104 B |
| SimdFilterAllWordsForSForeach |    127.7 ns |   2.48 ns |   2.95 ns |   1.00 |    0.00 | 0.0167 |     280 B |

Previous:
|                        Method |         Mean |       Error |      StdDev | Ratio |  Gen 0 | Allocated |
|------------------------------ |-------------:|------------:|------------:|------:|-------:|----------:|
|      ScalarFilterAllWordsForS | 209,131.7 ns | 4,182.20 ns | 3,912.03 ns | 1.000 |      - |     104 B |
| SimdFilterAllWordsForSForeach |     119.4 ns |     1.82 ns |     1.70 ns | 0.001 | 0.0167 |     280 B |

Previous: 
|                        Method |       Mean |   Error |  StdDev | Ratio |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------------------------------ |-----------:|--------:|--------:|------:|---------:|---------:|---------:|----------:|
|      ScalarFilterAllWordsForS | 1,013.6 us | 2.25 us | 1.88 us |  1.00 | 250.0000 | 250.0000 | 250.0000 |      2 MB |
| SimdFilterAllWordsForSForeach |   785.3 us | 6.60 us | 6.17 us |  0.78 | 250.0000 | 250.0000 | 250.0000 |      2 MB |

Previous:
|                        Method |       Mean |   Error |  StdDev | Ratio |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------------------------------ |-----------:|--------:|--------:|------:|---------:|---------:|---------:|----------:|
|      ScalarFilterAllWordsForS | 1,005.1 us | 5.83 us | 5.45 us |  1.00 | 250.0000 | 250.0000 | 250.0000 |      2 MB |
|        SimdFilterAllWordsForS |   809.8 us | 8.53 us | 7.97 us |  0.81 | 250.0000 | 250.0000 | 250.0000 |      2 MB |
| SimdFilterAllWordsForSForeach |   723.4 us | 5.45 us | 5.10 us |  0.72 | 230.4688 | 230.4688 | 230.4688 |      2 MB |