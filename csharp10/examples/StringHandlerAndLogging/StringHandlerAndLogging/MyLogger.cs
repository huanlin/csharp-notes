using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace StringHandlerAndLogging;

public class MyLogger
{
    public bool Enabled { get; set; }

    public string Log(string s)
    {
        if (Enabled)
        {
            return s;
        }

        return String.Empty;
    }


    public void Log([InterpolatedStringHandlerArgument("")] ref MyLoggerInterpolatedStringHandler handler)
    {
        if (Enabled)
        {
            string msg = handler.ToStringAndClear();
            handler.AppendLiteral("aa");
            // 將訊息寫入 log（略）
        }
    }


    public static void LogDebug(this MyLogger logger, [InterpolatedStringHandlerArgument("logger")] ref MyLoggerInterpolatedStringHandler handler)
    {

    }
}
