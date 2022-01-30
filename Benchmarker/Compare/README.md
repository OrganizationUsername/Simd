Current:
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