namespace HandyNetworking;

/// <summary>
/// Status of the connection to the session server
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Not connected
    /// </summary>
    Disconnected,

    /// <summary>
    /// Connected/Connecting, but not fully capable of sending/receiving packets
    /// </summary>
    Connecting,

    /// <summary>
    /// Connected to the server
    /// </summary>
    Connected
}