namespace HandyNetworking.Backends
{
    public interface INetBackend<TPeerId>
        where TPeerId : struct
    {
        /// <summary>
        /// Indicates if this peer is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Get statistics gathered by this backend
        /// </summary>
        INetStatistics Statistics { get; }

        void Subscribe(INetBackendEventListener<TPeerId> listener);

        void Subscribe(INetBackendPacketListener<TPeerId> listener);

        void StartServer(ushort port, IConnectionFilter? filter);

        void StartClient(string address, ushort port, ReadOnlySpan<byte> request);

        void Stop();

        void Update();

        bool Send(TPeerId peer, ReadOnlySpan<byte> payload, byte channel, PacketReliability reliability);
    }

    public interface INetBackendEventListener<in TPeerId>
        where TPeerId : struct
    {
        void PeerConnected(TPeerId peerId);

        void PeerDisconnected(TPeerId peerId);

        void PeerLatencyUpdate(TPeerId peerId, TimeSpan latency);
    }

    public interface INetBackendPacketListener<in TPeerId>
        where TPeerId : struct
    {
        void NetworkReceive(TPeerId sender, ReadOnlyMemory<byte> payload);
    }
}