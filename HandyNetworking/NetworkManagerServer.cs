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
    private class ServerSessionManager
        : INetBackendEventListener<TBackendId>, ISessionManager
    {
        private readonly ILog _logger;

        private readonly NetworkManager<TBackend, TBackendId> _networkManager;
        private int _nextPeerId = 1;

        private readonly Dictionary<TBackendId, PeerId> _idLookup = [];
        private readonly Dictionary<PeerId, TBackendId> _revIdLookup = [];

        private readonly List<NetPeer> _peers = [];
        public IReadOnlyList<NetPeer> Peers => _peers;

        public PeerId PeerId => new(1);
        public ConnectionStatus Status { get; } = ConnectionStatus.Connected;

        private readonly MemoryStream _cachedMemoryStream = new(1024);

        public ServerSessionManager(ILog logger, NetworkManager<TBackend, TBackendId> networkManager)
        {
            _logger = logger;
            _networkManager = networkManager;

            _peers.Add(new NetPeer(_networkManager, true, PeerId));
        }

        private bool TryGetPeer(PeerId peer, out TBackendId backendPeer)
        {
            return _revIdLookup.TryGetValue(peer, out backendPeer);
        }

        private void Broadcast<TPayload>(TPayload payload, byte channel, PacketReliability mode)
            where TPayload : IByteSerializable<TPayload>
        {
            lock (_cachedMemoryStream)
            {
                // Write into buffer
                var writer = new StreamByteWriter(_cachedMemoryStream.Clear());
                writer.WritePacketSerialized(PeerId, payload);
                var span = _cachedMemoryStream.GetSpan();

                // Send it to every client
                foreach (var kvp in _revIdLookup)
                    _networkManager.Send(kvp.Value, span, channel, mode);
            }
        }

        public void Send<TPayload>(PeerId destination, TPayload payload, byte channel, PacketReliability reliability)
            where TPayload : struct, IByteSerializable<TPayload>
        {
            if (!_revIdLookup.TryGetValue(destination, out var backendDest))
                return;

            Send(backendDest, payload, channel, reliability);
        }

        private void Send<TPayload>(TBackendId destination, TPayload payload, byte channel, PacketReliability reliability)
            where TPayload : struct, IByteSerializable<TPayload>
        {
            lock (_cachedMemoryStream)
            {
                // Write into buffer
                var writer = new StreamByteWriter(_cachedMemoryStream.Clear());
                writer.WritePacketSerialized(PeerId, payload);
                var span = _cachedMemoryStream.GetSpan();

                // Send to destination
                _networkManager.Send(destination, span, channel, reliability);
            }
        }

        void INetBackendEventListener<TBackendId>.PeerConnected(TBackendId peer)
        {
            var id = new PeerId(Interlocked.Increment(ref _nextPeerId));

            // Send a message to this peer, assigning their ID
            Send(peer, new AssignPeerId(id), 1, PacketReliability.ReliableOrdered);

            // Send messages to this new peer, introducing all other peers
            foreach (var netPeer in Peers)
                Send(peer, new RemotePeerJoined(netPeer.Id), 1, PacketReliability.ReliableUnordered);

            // Send a message to all peers, introducing this peer
            Broadcast(new RemotePeerJoined(id), 1, PacketReliability.ReliableOrdered);

            // Store this peer in the local collection
            var netObj = new NetPeer(_networkManager, false, id);
            _idLookup.Add(peer, id);
            _revIdLookup.Add(id, peer);
            _peers.Add(netObj);
            _networkManager.OnPeerConnected?.Invoke(netObj);
        }

        void INetBackendEventListener<TBackendId>.PeerDisconnected(TBackendId peerId)
        {
            if (!_idLookup.Remove(peerId, out var id))
                return;

            // Remove from local collections
            _revIdLookup.Remove(id);
            for (var i = 0; i < _peers.Count; i++)
            {
                if (_peers[i].Id == id)
                {
                    _networkManager.OnPeerDisconnected?.Invoke(_peers[i]);
                    _peers.RemoveAt(i);
                    break;
                }
            }

            // Send message informing everyone else
            Broadcast(
                new RemotePeerLeft(id),
                1,
                PacketReliability.ReliableOrdered
            );
        }

        void INetBackendEventListener<TBackendId>.PeerLatencyUpdate(TBackendId peerId, TimeSpan latency)
        {
            if (!_idLookup.TryGetValue(peerId, out var id))
                return;

            // Inform all peers about the new latency
            Broadcast(
                new UpdatePeerLatency(id, latency),
                1,
                PacketReliability.UnreliableSequenced
            );
        }

        public void Receive(PeerId sender, PacketTypes type, ref MemoryByteReader reader)
        {
            switch (type)
            {
                case PacketTypes.None:
                {
                    _logger.Error("Received PacketType `None`");
                    break;
                }

                case PacketTypes.Serialized:
                {
                    var typeId = reader.ReadUInt64();
                    _networkManager._packetManager.Receive(sender, typeId, reader);
                    break;
                }

                case PacketTypes.RelayedSingle:
                {
                    var header = reader.Read<MemoryByteReader, RelayHeader>();
                    if (header.Destination == PeerId)
                    {
                        if (!reader.ReadPacketHeader(out var innerType, out var innerSender))
                        {
                            _logger.Warn("Received packet with incorrect magic number");
                            return;
                        }

                        if (innerType == PacketTypes.RelayedSingle)
                        {
                            _logger.Warn("Received RelayedSingle packet which contained another RelayedSingle packet");
                            return;
                        }

                        Receive(innerSender, innerType, ref reader);
                    }
                    else
                    {
                        if (!TryGetPeer(header.Destination, out var backendDst))
                            return;

                        var unread = reader.ReadBytes(checked((int)reader.UnreadBytes));
                        _networkManager.Send(backendDst, unread, header.Channel, header.Reliability);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}