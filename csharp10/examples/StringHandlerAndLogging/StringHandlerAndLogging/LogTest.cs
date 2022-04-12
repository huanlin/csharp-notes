using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace StringHandlerAndLogging;

[MemoryDiagnoser]
public class LogTest
{
    MyLogger logger = new MyLogger() { Enabled = true };

    [Benchmark]
    public void Run()
    {
        var date = DateTime.Now;
        var s = logger.Log($"今天是 {date.Year} 年 {date.Month} {date.Day} 日");
    }
}

