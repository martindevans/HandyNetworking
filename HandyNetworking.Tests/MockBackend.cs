using HandyNetworking.Backends;

namespace HandyNetworking.Tests;

public class MockBackend
    : INetBackend<int>
{
    public int Id { get; }
    public bool IsRunning { get; private set; }
    public bool IsListening { get; private set; }

    private readonly MockBackendStatistics _stats = new();
    public INetStatistics Statistics => _stats;

    private readonly List<INetBackendEventListener<int>> _eventListeners = [ ];
    private readonly List<INetBackendPacketListener<int>> _packetListeners = [ ];

    private readonly Dictionary<string, MockBackend> _network;
    private readonly List<MockBackend> _connectedTo = [ ];
    private readonly Queue<(MockBackend, byte[])> _packetQueue = [ ];

    public MockBackend(int id, Dictionary<string, MockBackend> network)
    {
        Id = id;
        _network = network;
    }

    public void Subscribe(INetBackendEventListener<int> listener)
    {
        _eventListeners.Add(listener);
    }

    public void Subscribe(INetBackendPacketListener<int> listener)
    {
        _packetListeners.Add(listener);
    }

    public void StartServer(ushort port, IConnectionFilter? filter)
    {
        IsRunning = true;
        IsListening = true;

        if (filter != null)
            throw new NotImplementedException("filtering");
    }

    public void StartClient(string address, ushort port, ReadOnlySpan<byte> request)
    {
        var other = _network[address];
        if (!other.IsListening)
            throw new InvalidOperationException();

        IsRunning = true;

        other.EstablishConnection(this);
        EstablishConnection(other);

        if (request.Length > 0)
            throw new NotImplementedException("connection request data");
    }

    private void EstablishConnection(MockBackend other)
    {
        _connectedTo.Add(other);
        foreach (var netBackendEventListener in _eventListeners)
            netBackendEventListener.PeerConnected(other.Id);
    }

    public void Stop()
    {
        IsRunning = false;

        foreach (var mockBackend in _connectedTo)
            mockBackend.DestroyConnection(this);
        _connectedTo.Clear();
    }

    private void DestroyConnection(MockBackend other)
    {
        foreach (var netBackendEventListener in _eventListeners)
            netBackendEventListener.PeerDisconnected(other.Id);

        _connectedTo.Remove(other);
    }

    public void Update()
    {
        foreach (var (src, pkt) in _packetQueue)
            foreach (var listener in _packetListeners)
                listener.NetworkReceive(src.Id, pkt);

        _packetQueue.Clear();
    }

    private void EnqueuePacket(MockBackend sender, byte[] payload)
    {
        // Update sender and receiver stats
        sender._stats.BytesSent += payload.Length;
        sender._stats.PacketsSent++;
        _stats.BytesReceived += payload.Length;
        _stats.PacketsReceived++;

        _packetQueue.Enqueue((sender, payload));
    }

    public bool Send(int peer, ReadOnlySpan<byte> payload, byte channel, PacketReliability reliability)
    {
        _connectedTo
           .SingleOrDefault(a => a.Id == peer)
            ?.EnqueuePacket(this, payload.ToArray());

        return true;
    }

    public void Broadcast(ReadOnlySpan<byte> payload, byte channel, PacketReliability reliability)
    {
        foreach (var mockBackend in _connectedTo)
            mockBackend.EnqueuePacket(this, payload.ToArray());
    }
}

public class MockBackendStatistics
    : INetStatistics
{
    public long PacketsSent { get; set; }
    public long PacketsReceived { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public long PacketLoss { get; set; }
    public float PacketLossFactor { get; set; }
}