using HandyNetworking.Logging;

namespace HandyNetworking.Tests;

[TestClass]
[TestCategory("MockBackend")]
public class MockBackendSessionTests
    : SessionTester<MockBackend, int>
{
    protected override List<NetworkManager<MockBackend, int>> CreateNetwork(int peers, out string hostAddress, out ushort port, ILog? log = null)
    {
        hostAddress = "0";
        port = 0;

        // Build virtual network of backends
        var network = new Dictionary<string, MockBackend>();
        for (var i = 0; i < peers; i++)
            network.Add(i.ToString(), new MockBackend(i, network));

        // Create manager wrapping each backend "peer"
        return network.Select(a => new NetworkManager<MockBackend, int>(log ?? new MockLogger(), a.Value)).ToList();
    }
}