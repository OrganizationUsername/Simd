|                        Method |       Mean |   Error |  StdDev | Ratio |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|------------------------------ |-----------:|--------:|--------:|------:|---------:|---------:|---------:|----------:|
|      ScalarFilterAllWordsForS | 1,005.1 us | 5.83 us | 5.45 us |  1.00 | 250.0000 | 250.0000 | 250.0000 |      2 MB |
|        SimdFilterAllWordsForS |   809.8 us | 8.53 us | 7.97 us |  0.81 | 250.0000 | 250.0000 | 250.0000 |      2 MB |
| SimdFilterAllWordsForSForeach |   723.4 us | 5.45 us | 5.10 us |  0.72 | 230.4688 | 230.4688 | 230.4688 |      2 MB |