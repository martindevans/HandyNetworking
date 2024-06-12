namespace HandyNetworking.Extensions;

internal static class MemoryStreamExtensions
{
    public static Span<byte> GetSpan(this MemoryStream stream)
    {
        return stream.GetBuffer().AsSpan(0, checked((int)stream.Position));
    }

    public static MemoryStream Clear(this MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}