using System;
using BenchmarkDotNet.Attributes;

namespace StringInterpolationPerformanceTest
{
    [MemoryDiagnoser]
    public class StringInterpolBenchmark
    {
        [Benchmark]
        public string Today()
        {
            var d = DateTime.Now;
            return $"今天是 {d.Year} 年 {d.Month} 月 {d.Day} 日";
        }
    }
    /*
     * 測試步驟：
     * 1.確認專案的建置組態是 Release，而不是 Debug。
     * 2.把 Target Framework 設定為 .NET 5，然後執行。把執行結果保存下來。
     * 3.把 Target Framework 設定為 .NET 6，然後執行。把執行結果保存下來。
     * 4.觀察兩次執行結果的差異。例如：
     * 
     * | Method |     Mean |   Error |  StdDev |  Gen 0 | Allocated |
     * |------- |---------:|--------:|--------:|-------:|----------:|
     * |  Today | 188.1 ns | 2.67 ns | 2.37 ns | 0.0215 |     136 B |
     * 
     */
}
