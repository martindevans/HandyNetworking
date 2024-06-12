namespace HandyNetworking.Logging;

public interface ILog
{
    void Trace(string message);
    void Trace<TA>(string format, TA arg1);
    void Trace<TA, TB>(string format, TA arg1, TB arg2);
    void Trace<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3);
    void Trace<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4);
    void Trace<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5);

    void Debug(string message);
    void Debug<TA>(string format, TA arg1);
    void Debug<TA, TB>(string format, TA arg1, TB arg2);
    void Debug<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3);
    void Debug<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4);
    void Debug<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5);

    void Info(string message);
    void Info<TA>(string format, TA arg1);
    void Info<TA, TB>(string format, TA arg1, TB arg2);
    void Info<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3);
    void Info<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4);
    void Info<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5);

    void Warn(string message);
    void Warn<TA>(string format, TA arg1);
    void Warn<TA, TB>(string format, TA arg1, TB arg2);
    void Warn<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3);
    void Warn<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4);
    void Warn<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5);

    void Error(string message);
    void Error<TA>(string format, TA arg1);
    void Error<TA, TB>(string format, TA arg1, TB arg2);
    void Error<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3);
    void Error<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4);
    void Error<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5);
}