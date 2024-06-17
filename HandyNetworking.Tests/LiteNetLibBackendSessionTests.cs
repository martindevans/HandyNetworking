using HandyNetworking.LiteNetLibBackend;
using HandyNetworking.Logging;

namespace HandyNetworking.Tests;

[TestClass]
[TestCategory("LiteNetLib")]
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

    [TestMethod]
    public void CannotConnect()
    {
        var network = CreateNetwork(1, out var hostAddress, out var hostPort, new MockLogger() { ThrowWarning = true });

        // Start a client, ensure it connects to nowhere since the server does not exist
        var client = network[0];
        client.StartClient("localhost", 1234, []);
        for (var i = 0; i < 7500; i++)
        {
            Thread.Sleep(10);
            client.Update();

            if (client.Status == ConnectionStatus.Disconnected)
                break;
        }
        Assert.AreEqual(ConnectionStatus.Disconnected, client.Status);
    }
}