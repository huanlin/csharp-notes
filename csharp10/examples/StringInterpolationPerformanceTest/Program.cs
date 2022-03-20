using System;
using BenchmarkDotNet.Running;

namespace StringInterpolationPerformanceTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<StringInterpolBenchmark>();
        }
    }
}
