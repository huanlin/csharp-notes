using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace CustomInterpolatedStringHandler;

public class MyLogger
{
    public bool Enabled { get; set; }

    public void Log(string s)
    {
        if (Enabled)
        {
            // 將訊息寫入 log（略）
        }       
    }


    public void Log([InterpolatedStringHandlerArgument("")] ref MyLoggerInterpolatedStringHandler handler)
    {
        if (Enabled)
        {
            string msg = handler.ToStringAndClear();
            // 將訊息寫入 log（略）
        }
    }
}
