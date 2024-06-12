using HandyNetworking.LiteNetLibBackend;
using HandyNetworking.Logging;

namespace HandyNetworking.Tests;

[TestClass]
[TestCategory("LiteNetLib")]
[TestCategory("ActualSockets")]
public class LiteNetLibBackendSessionTests
    : SessionTester<Backend, int>
{
    protected override List<NetworkManager<Backend, int>> CreateNetwork(int peers, out string hostAddress, out ushort port, ILog? log = null)
    {
        log ??= new MockLogger { ThrowWarning = true };

        hostAddress = "localhost";
        port = (ushort)(DateTime.UtcNow.Microsecond % 0xFFFF);

        var network = new Dictionary<string, Backend>();
        for (var i = 0; i < peers; i++)
            network.Add(i.ToString(), new Backend(log));

        // Create manager wrapping each backend "peer"
        return network.Select(a => new NetworkManager<Backend, int>(log, a.Value)).ToList();
    }
}