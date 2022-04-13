using System;
using System.Runtime.CompilerServices;

namespace CustomInterpolStringHandler;

public class MyLogger
{
    public bool Enabled { get; set; }

    public void Log(string s)
    {
        if (Enabled)
        {
            Console.WriteLine(s);
        }
    }


    public void Log([InterpolatedStringHandlerArgument("")] ref MyLoggerInterpolatedStringHandler handler)
    {
        if (Enabled)
        {
            string msg = handler.ToStringAndClear();
            Console.WriteLine(msg);
        }
    }
}

public static class MyLoggerExtension
{
    // 如果寫成靜態方法
    public static void LogDebug(
        this MyLogger logger, 
        [InterpolatedStringHandlerArgument("logger")] ref MyLoggerInterpolatedStringHandler handler)
    {
        if (logger.Enabled)
        {
            string msg = handler.ToStringAndClear();
            Console.WriteLine(msg);
        }
    }
}