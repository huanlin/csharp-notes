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

// 擴充方法
public static class MyLoggerExtension
{
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