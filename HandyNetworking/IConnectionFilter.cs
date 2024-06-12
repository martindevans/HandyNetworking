namespace HandyNetworking;

/// <summary>
/// Accept or deny connection requests
/// </summary>
public interface IConnectionFilter
{
    /// <summary>
    /// Decide if a connection should be accept
    /// </summary>
    /// <param name="packet">The packet send as part of the request to join</param>
    /// <returns></returns>
    bool AcceptConnection(ReadOnlyMemory<byte> packet);
}