using HandyNetworking.Backends;
using HandyNetworking.Extensions;
using HandyNetworking.Logging;
using HandyNetworking.Protocol;
using HandySerialization;
using HandySerialization.Extensions;
using HandySerialization.Wrappers;

namespace HandyNetworking;

public partial class NetworkManager<TBackend, TBackendId>
{
    private class ClientSessionManager
        : ISessionManager
    {
        private readonly ILog _logger;
        private readonly NetworkManager<TBackend, TBackendId> _networkManager;

        private readonly List<NetPeer> _peers = [];
        public IReadOnlyList<NetPeer> Peers => _peers;

        private TBackendId? _serverId;

        public ConnectionStatus Status { get; private set; }

        private PeerId _id;
        public PeerId PeerId
        {
            get
            {
                if (Status != ConnectionStatus.Connected)
                    throw new InvalidOperationException("Cannot get PeerId until client is connected");
                return _id;
            }
        }

        private readonly MemoryStream _cachedMemoryStream = new(1024);

        public ClientSessionManager(ILog logger, NetworkManager<TBackend, TBackendId> networkManager)
        {
            _logger = logger;
            _networkManager = networkManager;

            _networkManager.Subscribe<AssignPeerId>(OnAssignId);
            _networkManager.Subscribe<RemotePeerJoined>(OnRemotePeerJoined);
            _networkManager.Subscribe<RemotePeerLeft>(OnRemotePeerLeft);
            _networkManager.Subscribe<UpdatePeerLatency>(OnUpdateLatency);

            Status = ConnectionStatus.Connecting;
        }

        private void OnUpdateLatency(PeerId sender, UpdatePeerLatency pkt)
        {
            foreach (var netPeer in _peers)
                if (netPeer.Id == pkt.Id)
                    netPeer.Latency = pkt.Latency;
        }

        private void OnAssignId(PeerId sender, AssignPeerId pkt)
        {
            _id = pkt.Id;

            // Check if we already know of this peer, if not then add it to the session
            foreach (var netPeer in _peers)
                if (netPeer.Id == pkt.Id)
                    return;

            var peer = new NetPeer(_networkManager, true, _id);
            _peers.Add(peer);
            _networkManager.OnPeerConnected?.Invoke(peer);

            Status = ConnectionStatus.Connected;
        }

        private void OnRemotePeerJoined(PeerId sender, RemotePeerJoined pkt)
        {
            // Check if we already know of this peer
            foreach (var netPeer in _peers)
                if (netPeer.Id == pkt.Id)
                    return;

            var peer = new NetPeer(_networkManager, false, pkt.Id);
            _peers.Add(peer);
            _networkManager.OnPeerConnected?.Invoke(peer);
        }

        private void OnRemotePeerLeft(PeerId sender, RemotePeerLeft pkt)
        {
            for (var i = 0; i < _peers.Count; i++)
            {
                if (_peers[i].Id == pkt.Id)
                {
                    var peer = _peers[i];
                    _peers.RemoveAt(i);
                    _networkManager.OnPeerDisconnected?.Invoke(peer);
                    return;
                }
            }
        }

        public void Receive(PeerId sender, TBackendId _, PacketTypes type, ref MemoryByteReader reader)
        {
            switch (type)
            {
                case PacketTypes.None:
                    _logger.Error("Received PacketType `None`");
                    break;

                case PacketTypes.Serialized:
                    var typeId = reader.ReadUInt64();
                    _networkManager._packetManager.Receive(sender, typeId, reader);
                    break;

                case PacketTypes.RelayedSingle:
                    _logger.Error("Client peer received PacketType `RelayedSingle`");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public void Send<TPayload>(PeerId destination, TPayload payload, byte channel, PacketReliability reliability)
            where TPayload : struct, IByteSerializable<TPayload>
        {
            if (!_serverId.HasValue)
                throw new InvalidOperationException("Cannot send packet before server connection");
            if (Status != ConnectionStatus.Connected)
                throw new InvalidOperationException("Cannot send packet before connected");
            if (destination == PeerId)
                return;

            // Send a packet to the server, relaying this packet to it's final destination.
            lock (_cachedMemoryStream)
            {
                var writer = new StreamByteWriter(_cachedMemoryStream.Clear());
                writer.WritePacketRelayHeader(PeerId, new RelayHeader(destination, channel, reliability));
                writer.WritePacketSerialized(PeerId, payload);
                var span = _cachedMemoryStream.GetSpan();

                _networkManager.Send(_serverId.Value, span, channel, reliability);
            }
        }

        public void Broadcast<TPayload>(TPayload payload, byte channel, PacketReliability reliability)
            where TPayload : struct, IByteSerializable<TPayload>
        {
            if (!_serverId.HasValue)
                throw new InvalidOperationException("Cannot send packet before server connection");
            if (Status != ConnectionStatus.Connected)
                throw new InvalidOperationException("Cannot send packet before connected");

            // Send a packet to the server, relaying to other peers. Use peer id = 0 to indicate broadcast
            lock (_cachedMemoryStream)
            {
                var writer = new StreamByteWriter(_cachedMemoryStream.Clear());
                writer.WritePacketRelayHeader(PeerId, new RelayHeader(new PeerId(0), channel, reliability));
                writer.WritePacketSerialized(PeerId, payload);
                var span = _cachedMemoryStream.GetSpan();

                _networkManager.Send(_serverId.Value, span, channel, reliability);
            }
        }

        void INetBackendEventListener<TBackendId>.PeerConnected(TBackendId peerId)
        {
            _serverId = peerId;
        }

        void INetBackendEventListener<TBackendId>.PeerDisconnected(TBackendId peerId)
        {
            _networkManager.Stop();
        }

        void INetBackendEventListener<TBackendId>.PeerLatencyUpdate(TBackendId peerId, TimeSpan latency)
        {
        }
    }
}