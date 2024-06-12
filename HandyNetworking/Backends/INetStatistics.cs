namespace HandyNetworking.Backends
{
    public interface INetStatistics
    {
        long PacketsSent { get; }

        long PacketsReceived { get; }

        long BytesSent { get; }

        long BytesReceived { get; }

        long PacketLoss { get; }

        float PacketLossFactor { get; }
    }
}