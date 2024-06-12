using System.Net;
using System.Net.Sockets;
using HandyNetworking.Backends;
using HandyNetworking.LiteNetLibBackend.Extensions;
using HandyNetworking.Logging;
using LiteNetLib;
using LiteNetLib.Layers;
using LiteNetLib.Utils;

namespace HandyNetworking.LiteNetLibBackend
{
    public class Backend
        : INetBackend<int>, INetEventListener
    {
        private readonly ILog _logger;
        private readonly NetManager _manager;

        private readonly List<INetBackendEventListener<int>> _eventsListeners = [ ];
        private readonly List<INetBackendPacketListener<int>> _packetListeners = [ ];

        public bool IsRunning => _manager.IsRunning;

        public bool AcceptConnections { get; private set; }
        private IConnectionFilter? _connectionFilter;

        private readonly Statistics _stats;
        public INetStatistics Statistics => _stats;

        public Backend(ILog logger)
        {
            _logger = logger;
            _manager = new NetManager(this)
            {
                ChannelsCount = 64,
                AllowPeerAddressChange = false,
                EnableStatistics = true,
                UnconnectedMessagesEnabled = false,
            };

            _stats = new Statistics(_manager.Statistics);
        }

        public void Subscribe(INetBackendEventListener<int> listener)
        {
            _eventsListeners.Add(listener);
        }

        public void Subscribe(INetBackendPacketListener<int> listener)
        {
            _packetListeners.Add(listener);
        }

        public void StartServer(ushort port, IConnectionFilter? connectionFilter)
        {
            _connectionFilter = connectionFilter;
            _manager.Start(port);
            AcceptConnections = true;
        }

        public void StartClient(string address, ushort port, ReadOnlySpan<byte> request)
        {
            var writer = new NetDataWriter(true, request.Length + sizeof(int));
            writer.Put(request.Length);
            for (var i = 0; i < request.Length; i++)
                writer.Put(request[i]);

            _manager.Start();
            _manager.Connect(address, port, writer);
            AcceptConnections = false;
        }

        public void Stop()
        {
            _manager.Stop(true);
            _connectionFilter = null;
        }

        public void Update()
        {
            _manager.PollEvents();
        }

        public bool Send(int peer, ReadOnlySpan<byte> payload, byte channel, PacketReliability reliability)
        {
            channel = (byte)(channel & 0b11_1111);

            if (!_manager.TryGetPeerById(peer, out var netPeer))
                return false;

            netPeer.Send(payload, channel, reliability.ToDeliveryMethod());
            return true;
        }

        void INetEventListener.OnPeerConnected(LiteNetLib.NetPeer peer)
        {
            foreach (var listener in _eventsListeners)
                listener.PeerConnected(peer.Id);
        }

        void INetEventListener.OnPeerDisconnected(LiteNetLib.NetPeer peer, DisconnectInfo disconnectInfo)
        {
            foreach (var listener in _eventsListeners)
                listener.PeerDisconnected(peer.Id);
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _logger.Error($"Network Error '{socketError}'@{endPoint}");
        }

        void INetEventListener.OnNetworkReceive(LiteNetLib.NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            var memory = reader.RawData.AsMemory(reader.Position, reader.AvailableBytes);
            foreach (var listener in _packetListeners)
                listener.NetworkReceive(peer.Id, memory);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            throw new NotSupportedException();
        }

        void INetEventListener.OnNetworkLatencyUpdate(LiteNetLib.NetPeer peer, int latency)
        {
            foreach (var listener in _eventsListeners)
                listener.PeerLatencyUpdate(peer.Id, TimeSpan.FromMilliseconds(latency));
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            if (!AcceptConnections)
            {
                request.Reject();
                return;
            }

            var accept = true;
            if (_connectionFilter != null)
            {
                var length = checked((ushort)request.Data.GetInt());
                var requestData = new byte[length];
                for (var i = 0; i < length; i++)
                    requestData[i] = request.Data.GetByte();

                accept = _connectionFilter.AcceptConnection(requestData);
            }

            if (accept)
                request.Accept();
            else
                request.Reject();
        }
    }
}
