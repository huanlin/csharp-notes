using BenchmarkDotNet.Running;
using StringHandlerAndLogging;

//BenchmarkRunner.Run<LogTest>();

var logger = new MyLogger() { Enabled = true };

logger.Log($"今天是 {DateTime.Now.Day}");
