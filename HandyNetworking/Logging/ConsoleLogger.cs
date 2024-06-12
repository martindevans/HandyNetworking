namespace HandyNetworking.Logging;

public class ConsoleLogger
    : ILog
{
    public bool TraceEnabled { get; set; } = false;
    public bool DebugEnabled { get; set; } = false;
    public bool InfoEnabled { get; set; } = true;
    public bool WarnEnabled { get; set; } = true;
    public bool ErrorEnabled { get; set; } = true;

    public void Trace(string message)
    {
        if (TraceEnabled)
            Console.WriteLine(message);
    }

    public void Trace<TA>(string format, TA arg1)
    {
        if (TraceEnabled)
            Console.WriteLine(format, arg1);
    }

    public void Trace<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (TraceEnabled)
            Console.WriteLine(format, arg1, arg2);
    }

    public void Trace<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (TraceEnabled)
            Console.WriteLine(format, arg1, arg2, arg3);
    }

    public void Trace<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (TraceEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4);
    }

    public void Trace<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (TraceEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4, arg5);
    }

    public void Debug(string message)
    {
        if (DebugEnabled)
            Console.WriteLine(message);
    }

    public void Debug<TA>(string format, TA arg1)
    {
        if (DebugEnabled)
            Console.WriteLine(format, arg1);
    }

    public void Debug<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (DebugEnabled)
            Console.WriteLine(format, arg1, arg2);
    }

    public void Debug<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (DebugEnabled)
            Console.WriteLine(format, arg1, arg2, arg3);
    }

    public void Debug<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (DebugEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4);
    }

    public void Debug<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (DebugEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4, arg5);
    }

    public void Info(string message)
    {
        if (InfoEnabled)
            Console.WriteLine(message);
    }

    public void Info<TA>(string format, TA arg1)
    {
        if (InfoEnabled)
            Console.WriteLine(format, arg1);
    }

    public void Info<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (InfoEnabled)
            Console.WriteLine(format, arg1, arg2);
    }

    public void Info<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (InfoEnabled)
            Console.WriteLine(format, arg1, arg2, arg3);
    }

    public void Info<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (InfoEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4);
    }

    public void Info<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (InfoEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4, arg5);
    }

    public void Warn(string message)
    {
        if (WarnEnabled)
            Console.WriteLine(message);
    }

    public void Warn<TA>(string format, TA arg1)
    {
        if (WarnEnabled)
            Console.WriteLine(format, arg1);
    }

    public void Warn<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (WarnEnabled)
            Console.WriteLine(format, arg1, arg2);
    }

    public void Warn<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (WarnEnabled)
            Console.WriteLine(format, arg1, arg2, arg3);
    }

    public void Warn<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (WarnEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4);
    }

    public void Warn<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (WarnEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4, arg5);
    }

    public void Error(string message)
    {
        if (ErrorEnabled)
            Console.WriteLine(message);
    }

    public void Error<TA>(string format, TA arg1)
    {
        if (ErrorEnabled)
            Console.WriteLine(format, arg1);
    }

    public void Error<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (ErrorEnabled)
            Console.WriteLine(format, arg1, arg2);
    }

    public void Error<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (ErrorEnabled)
            Console.WriteLine(format, arg1, arg2, arg3);
    }

    public void Error<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (ErrorEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4);
    }

    public void Error<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (ErrorEnabled)
            Console.WriteLine(format, arg1, arg2, arg3, arg4, arg5);
    }
}