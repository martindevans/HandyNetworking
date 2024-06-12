using HandyNetworking.Backends;
using LiteNetLib;

namespace HandyNetworking.LiteNetLibBackend;

public class Statistics
    : INetStatistics
{
    public long PacketsSent => _stats.PacketsSent;
    public long PacketsReceived => _stats.PacketsReceived;
    public long BytesSent => _stats.BytesSent;
    public long BytesReceived => _stats.BytesReceived;
    public long PacketLoss => _stats.PacketLoss;
    public float PacketLossFactor => _stats.PacketLossPercent / 100f;

    private readonly NetStatistics _stats;

    public Statistics(NetStatistics stats)
    {
        _stats = stats;
    }
}