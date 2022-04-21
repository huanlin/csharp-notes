using System;
using System.Runtime.CompilerServices;

namespace CustomInterpolStringHandler;

// ref: https://docs.microsoft.com/zh-tw/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-interpolated-strings#the-handler-pattern

[InterpolatedStringHandler]
public ref struct MyLoggerInterpolatedStringHandler
{
    private DefaultInterpolatedStringHandler _innerHandler;

    public MyLoggerInterpolatedStringHandler(int literalLength, int formattedCount,
        MyLogger logger, out bool handlerIsValid)
    {
        if (!logger.Enabled)
        {
            _innerHandler = default;
            handlerIsValid = false;
            return;
        }

        _innerHandler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
        handlerIsValid = true;
    }

    public void AppendLiteral(string msg)
    {
        _innerHandler.AppendLiteral(msg);
    }

    public void AppendFormatted<T>(T msg)
    {
        _innerHandler.AppendFormatted(msg);
    }

    public string ToStringAndClear()
    {
        return _innerHandler.ToStringAndClear();
    }
}
