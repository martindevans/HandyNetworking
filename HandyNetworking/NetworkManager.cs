using HandyNetworking.Backends;
using HandyNetworking.Serialization;
using HandySerialization;
using HandySerialization.Wrappers;
using HandyNetworking.Logging;
using HandyNetworking.Protocol;
using HandySerialization.Extensions;

namespace HandyNetworking;

public partial class NetworkManager<TBackend, TBackendId>
    : INetBackendPacketListener<TBackendId>, ISender
    where TBackend : INetBackend<TBackendId>
    where TBackendId : struct
{
    #region fields and properties
    private readonly ILog _logger;
    private readonly TBackend _backend;

    private ISessionManager? _session;
    public IReadOnlyList<NetPeer> Peers => _session?.Peers ?? [];

    public event Action<NetPeer>? OnPeerConnected;
    public event Action<NetPeer>? OnPeerDisconnected;

    public bool IsRunning => _backend.IsRunning;
    public bool IsServer { get; private set; }
    public ConnectionStatus Status => _session?.Status ?? ConnectionStatus.Disconnected;

    public INetStatistics Statistics => _backend.Statistics;

    private readonly SerializedPacketManager _packetManager = new();

    public PeerId LocalPeerId
    {
        get
        {
            if (Status != ConnectionStatus.Connected)
                throw new InvalidOperationException("Cannot get LocalPeerId when Status is not Connected");
            return _session!.PeerId;
        }
    }

    #endregion

    public NetworkManager(ILog? logger, TBackend backend)
    {
        _logger = logger ?? new ConsoleLogger();
        _backend = backend;
    }

    #region start/stop
    public void StartServer(ushort port, IConnectionFilter? connectionFilter = null)
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot `StartServer()`: NetManager is already running");

        IsServer = true;

        _session = new ServerSessionManager(_logger, this);

        _backend.Subscribe(_session);
        _backend.Subscribe(this);
        _backend.StartServer(port, connectionFilter);
    }

    public void StartClient(string address, ushort port, ReadOnlySpan<byte> requestPacket)
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot `StartClient()`: NetManager is already running");

        IsServer = false;

        _session = new ClientSessionManager(_logger, this);

        _backend.Subscribe(_session);
        _backend.Subscribe(this);
        _backend.StartClient(address, port, requestPacket);
    }

    public void Stop()
    {
        if (IsRunning)
            _backend.Stop();
        _session = null;
        _packetManager.Clear();
    }
    #endregion

    public void Update()
    {
        _backend.Update();
    }

    #region send/receive
    public IDisposable Subscribe<TPayload>(Action<PeerId, TPayload> callback)
        where TPayload : struct, IByteSerializable<TPayload>
    {
        return _packetManager.Subscribe(callback);
    }

    public void Send<T>(PeerId dst, T payload, byte channel, PacketReliability reliability)
        where T : struct, IByteSerializable<T>
    {
        _session!.Send(dst, payload, channel, reliability);
    }

    private void Send(TBackendId dst, ReadOnlySpan<byte> payload, byte channel, PacketReliability reliability)
    {
        _backend.Send(dst, payload, channel, reliability);
    }
    #endregion

    #region event handlers
    void INetBackendPacketListener<TBackendId>.NetworkReceive(TBackendId backendSender, ReadOnlyMemory<byte> payload)
    {
        var reader = new MemoryByteReader(payload);
        if (!reader.ReadPacketHeader(out var type, out var sender))
        {
            _logger.Warn("Received packet with incorrect magic number");
            return;
        }

        if (_session == null)
        {
            _logger.Warn("Received packet when _session is null");
            return;
        }

        _session.Receive(sender, type, ref reader);
    }

    private interface IPacketHandler
    {
        void Clear();

        void Receive(PeerId sender, MemoryByteReader reader);
    }

    private class PacketHandler<TPayload>
        : IPacketHandler
        where TPayload : struct, IByteSerializable<TPayload>
    {
        private int _nextId = 0;
        private readonly List<(int, Action<PeerId, TPayload>)> _callbacks = [ ];

        public IDisposable Add(Action<PeerId, TPayload> callback)
        {
            var id = _nextId++;
            _callbacks.Add((id, callback));
            return new RemoveDispose<TPayload>(id, _callbacks);
        }

        public void Clear()
        {
            _callbacks.Clear();
        }

        public void Receive(PeerId sender, MemoryByteReader reader)
        {
            var payload = reader.Read<MemoryByteReader, TPayload>();
            foreach (var (_, callback) in _callbacks)
                callback(sender, payload);
        }
    }

    private class RemoveDispose<TPayload>
        : IDisposable
    {
        private readonly int _value;
        private readonly List<(int, Action<PeerId, TPayload>)> _items;

        public RemoveDispose(int value, List<(int, Action<PeerId, TPayload>)> items)
        {
            _value = value;
            _items = items;
        }

        public void Dispose()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Item1 == _value)
                {
                    _items.RemoveAt(i);
                    return;
                }
            }
        }
    }

    private class SerializedPacketManager
    {
        private readonly Dictionary<ulong, IPacketHandler> _handlers = [];

        public IDisposable Subscribe<TPayload>(Action<PeerId, TPayload> callback)
            where TPayload : struct, IByteSerializable<TPayload>
        {
            if (!_handlers.TryGetValue(UniversalTypeId<TPayload>.Id, out var handler))
            {
                handler = new PacketHandler<TPayload>();
                _handlers.Add(UniversalTypeId<TPayload>.Id, handler);
            }

            return ((PacketHandler<TPayload>)handler).Add(callback);
        }

        public void Clear<TPayload>()
            where TPayload : struct, IByteSerializable<TPayload>
        {
            if (!_handlers.TryGetValue(UniversalTypeId<TPayload>.Id, out var handler))
                return;

            handler.Clear();
        }

        public void Clear()
        {
            _handlers.Clear();
        }

        public void Receive(PeerId sender, ulong typeId, MemoryByteReader reader)
        {
            if (!_handlers.TryGetValue(typeId, out var handler))
                return;
            handler.Receive(sender, reader);
        }
    }
    #endregion

    #region session management
    private readonly record struct AssignPeerId(PeerId Id)
        : IByteSerializable<AssignPeerId>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Id);
        }

        public AssignPeerId Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.Read<TReader, PeerId>()
            );
        }
    }

    private readonly record struct RemotePeerJoined(PeerId Id)
        : IByteSerializable<RemotePeerJoined>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Id);
        }

        public RemotePeerJoined Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.Read<TReader, PeerId>()
            );
        }
    }

    private readonly record struct RemotePeerLeft(PeerId Id)
        : IByteSerializable<RemotePeerLeft>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Id);
        }

        public RemotePeerLeft Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.Read<TReader, PeerId>()
            );
        }
    }

    private readonly record struct UpdatePeerLatency(PeerId Id, TimeSpan Latency)
        : IByteSerializable<UpdatePeerLatency>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Id);
            writer.Write(Latency);
        }

        public UpdatePeerLatency Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new(
                reader.Read<TReader, PeerId>(),
                reader.ReadTimeSpan()
            );
        }
    }

    private interface ISessionManager
        : INetBackendEventListener<TBackendId>
    {
        IReadOnlyList<NetPeer> Peers { get; }

        PeerId PeerId { get; }

        ConnectionStatus Status { get; }

        void Receive(PeerId sender, PacketTypes type, ref MemoryByteReader reader);

        void Send<T>(PeerId destination, T payload, byte channel, PacketReliability reliability)
            where T : struct, IByteSerializable<T>;
    }
    #endregion
}

internal interface ISender
{
    public void Send<T>(PeerId dst, T payload, byte channel, PacketReliability reliability)
        where T : struct, IByteSerializable<T>;
}