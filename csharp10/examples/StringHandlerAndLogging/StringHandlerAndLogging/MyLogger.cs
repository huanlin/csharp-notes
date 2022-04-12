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


    public string Log([InterpolatedStringHandlerArgument("")] ref MyLoggerInterpolatedStringHandler handler)
    {
        if (Enabled)
        {
            return handler.ToStringAndClear();
        }

        return String.Empty;
    }

}
