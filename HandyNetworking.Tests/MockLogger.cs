using HandyNetworking.Logging;

namespace HandyNetworking.Tests;

public class MockLogger
    : ILog
{
    public bool ThrowWarning { get; set; }
    public bool ThrowError { get; set; } = true;

    public void Trace(string message)
    {
    }

    public void Trace<TA>(string format, TA arg1)
    {
    }

    public void Trace<TA, TB>(string format, TA arg1, TB arg2)
    {
    }

    public void Trace<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
    }

    public void Trace<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
    }

    public void Trace<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
    }

    public void Debug(string message)
    {
    }

    public void Debug<TA>(string format, TA arg1)
    {
    }

    public void Debug<TA, TB>(string format, TA arg1, TB arg2)
    {
    }

    public void Debug<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
    }

    public void Debug<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
    }

    public void Debug<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
    }

    public void Info(string message)
    {
        
    }

    public void Info<TA>(string format, TA arg1)
    {
        
    }

    public void Info<TA, TB>(string format, TA arg1, TB arg2)
    {
        
    }

    public void Info<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        
    }

    public void Info<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        
    }

    public void Info<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        
    }

    public void Warn(string message)
    {
        if (ThrowWarning)
            Assert.Fail(message);
    }

    public void Warn<TA>(string format, TA arg1)
    {
        if (ThrowWarning)
            Assert.Fail(format, arg1);
    }

    public void Warn<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (ThrowWarning)
            Assert.Fail(format, arg1, arg2);
    }

    public void Warn<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (ThrowWarning)
            Assert.Fail(format, arg1, arg2, arg3);
    }

    public void Warn<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (ThrowWarning)
            Assert.Fail(format, arg1, arg2, arg3, arg4);
    }

    public void Warn<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (ThrowWarning)
            Assert.Fail(format, arg1, arg2, arg3, arg4, arg5);
    }

    public void Error(string message)
    {
        if (ThrowError)
            Assert.Fail(message);
    }

    public void Error<TA>(string format, TA arg1)
    {
        if (ThrowError)
            Assert.Fail(format, arg1);
    }

    public void Error<TA, TB>(string format, TA arg1, TB arg2)
    {
        if (ThrowError)
            Assert.Fail(format, arg1, arg2);
    }

    public void Error<TA, TB, TC>(string format, TA arg1, TB arg2, TC arg3)
    {
        if (ThrowError)
            Assert.Fail(format, arg1, arg2, arg3);
    }

    public void Error<TA, TB, TC, TD>(string format, TA arg1, TB arg2, TC arg3, TD arg4)
    {
        if (ThrowError)
            Assert.Fail(format, arg1, arg2, arg3, arg4);
    }

    public void Error<TA, TB, TC, TD, TE>(string format, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5)
    {
        if (ThrowError)
            Assert.Fail(format, arg1, arg2, arg3, arg4, arg5);
    }
}