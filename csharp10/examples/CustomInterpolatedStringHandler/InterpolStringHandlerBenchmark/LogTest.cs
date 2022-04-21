using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace CustomInterpolatedStringHandler;

[MemoryDiagnoser]
public class LogTest
{
    MyLogger logger = new MyLogger() { Enabled = true };

    [Benchmark]
    public void Run()
    {
        var date = DateTime.Now;
        logger.Log($"今天是 {date.Month} 月 {date.Day} 日");
    }
}

