using HandyNetworking.Backends;
using HandyNetworking.Logging;
using HandySerialization;
using HandySerialization.Extensions;

namespace HandyNetworking.Tests;

public abstract class SessionTester<TBackend, TBackendPeerId>
    where TBackend : INetBackend<TBackendPeerId>
    where TBackendPeerId : struct
{
    protected abstract List<NetworkManager<TBackend, TBackendPeerId>> CreateNetwork(int peers, out string hostAddress, out ushort hostPort, ILog? log = null);

    private List<NetworkManager<TBackend, TBackendPeerId>> CreateSession(int peerCount)
    {
        var network = CreateNetwork(peerCount, out var hostAddress, out var hostPort, new MockLogger() { ThrowWarning = true });

        // Start first one listening
        network[0].StartServer(hostPort);
        Assert.IsTrue(network[0].IsRunning);
        Assert.IsTrue(network[0].IsServer);

        // Start all others connecting to server
        foreach (var manager in network)
        {
            if (manager.IsRunning)
                continue;

            // Connect to server
            manager.StartClient(hostAddress, hostPort, []);
            Assert.IsTrue(manager.IsRunning);
            Assert.IsFalse(manager.IsServer);

            // Pump packets for everyone
            Pump(network);
        }

        // Update all managers to get packets flowing
        Pump(network);

        // Now check every manager has the total session state
        foreach (var item in network)
            Assert.AreEqual(peerCount, item.Peers.Count);

        // Check every peer has been assigned a unique ID
        foreach (var item in network)
            Assert.IsTrue(item.LocalPeerId.Value > 0);
        Assert.AreEqual(10, network.Select(a => a.LocalPeerId!.Value).Distinct().Count());

        return network;
    }

    private static void DestroySession(List<NetworkManager<TBackend, TBackendPeerId>> session, bool test = true)
    {
        for (var i = session.Count - 1; i >= 0; i--)
        {
            var item = session[i];
            item.Stop();

            for (var j = 0; j < 10; j++)
            {
                Thread.Sleep(15);
                foreach (var inner in session)
                    inner.Update();
            }

            if (test)
                Assert.AreEqual(i, session[0].Peers.Count);
        }
    }

    private static void Pump(List<NetworkManager<TBackend, TBackendPeerId>> session, int count = 10)
    {
        for (var i = 0; i < count; i++)
        {
            Thread.Sleep(5);
            foreach (var item in session)
                item.Update();
        }
    }

    [TestMethod]
    public void EstablishSession()
    {
        var session = CreateSession(10);
        try
        {
            foreach (var manager in session)
            {
                Assert.IsTrue(manager.IsRunning);
                Assert.IsTrue(manager.Statistics.PacketsReceived > 0 || manager.Statistics.PacketsSent > 0);
                Assert.IsTrue(manager.Statistics.BytesReceived > 0 || manager.Statistics.BytesSent > 0);

                // Check there is a single peer object with the local ID, and it has the local flag
                var local = manager.Peers.Single(a => a.Id == manager.LocalPeerId);
                Assert.IsTrue(local.IsLocal);

                // Check every peer only has the local flag if it has the local ID
                foreach (var peer in manager.Peers)
                    Assert.AreEqual(peer.Id == manager.LocalPeerId, peer.IsLocal);
            }
        }
        finally
        {
            DestroySession(session);
        }
    }

    [TestMethod]
    public void SendPacketPairs()
    {
        var session = CreateSession(10);
        try
        {
            // We'll never send this packet, so receiving it is always an error!
            for (var i = 0; i < session.Count; i++)
                session[i].Subscribe<TestPacket2>((_, _) => Assert.Fail("Received TestPacket2"));

            var serverIdx = session.FindIndex(a => a.IsServer);

            for (var i = 0; i < session.Count; i++)
            {
                if (session[i].IsServer)
                {
                    for (var j = 0; j < session.Count; j++)
                    {
                        if (i == j)
                            continue;
                        SendPair(i, j);
                    }
                }
                else
                {
                    SendPair(i, serverIdx);
                }

                
            }
        }
        finally
        {
            DestroySession(session);
        }

        void SendPair(int from, int to)
        {
            var received = new List<TestPacket1>();
            var subs = new List<IDisposable>();
            for (var i = 0; i < session.Count; i++)
            {
                if (i == to)
                    subs.Add(session[i].Subscribe<TestPacket1>((_, pkt) => received.Add(pkt)));
                else
                    subs.Add(session[i].Subscribe<TestPacket1>((_, _) => Assert.Fail("Received TestPacket1 at wrong peer")));
            }

            session[from].Send(session[to].LocalPeerId, new TestPacket1(from + to), 0, PacketReliability.ReliableOrdered);

            Pump(session);

            Assert.AreEqual(1, received.Count);
            Assert.AreEqual(from + to, received.Single().Value);

            foreach (var disposable in subs)
                disposable.Dispose();
        }
    }

    [TestMethod]
    public void TeardownSession()
    {
        var session = CreateSession(10);
        try
        {
            var inSession = session.Select(a => a.LocalPeerId).ToHashSet();
            var notInSession = new HashSet<PeerId>();

            // Disconnect peers one by one
            var count = session.Count;
            for (var i = 1; i < session.Count; i++)
            {
                var stopping = session[i];
                Assert.IsNotNull(stopping.LocalPeerId);

                // Keep track of who is meant to be in the session
                Assert.IsTrue(inSession.Remove(stopping.LocalPeerId));
                Assert.IsTrue(notInSession.Add(stopping.LocalPeerId));

                // Stop this peer
                stopping.Stop();
                Assert.AreEqual(0, stopping.Peers.Count);
                Assert.AreEqual(ConnectionStatus.Disconnected, stopping.Status);
                count--;

                // Pump packets
                Pump(session);

                // Check that the server has the expected peers
                Assert.AreEqual(count, session[0].Peers.Count);
                Assert.IsTrue(inSession.SetEquals(session[0].Peers.Select(a => a.Id)));

                // Check that every other peer has same set of peers as the server
                for (var j = (i + 1); j < session.Count; j++)
                    Assert.IsTrue(session[0].Peers.Select(a => a.Id).ToHashSet().SetEquals(session[j].Peers.Select(a => a.Id)));
            }
        }
        finally
        {
            DestroySession(session, false);
        }
    }

    private readonly record struct TestPacket1(int Value)
        : IByteSerializable<TestPacket1>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Value);
        }

        public TestPacket1 Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.ReadInt32()
            );
        }
    }

    private readonly record struct TestPacket2(int Value)
        : IByteSerializable<TestPacket2>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Value);
        }

        public TestPacket2 Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.ReadInt32()
            );
        }
    }
}