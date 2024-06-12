using HandySerialization;
using HandySerialization.Extensions;

namespace HandyNetworking.Protocol;

internal readonly record struct RelayHeader
    : IByteSerializable<RelayHeader>
{
    public readonly PeerId Destination;
    public readonly byte Channel;
    public readonly PacketReliability Reliability;

    public RelayHeader(PeerId destination, byte channel, PacketReliability reliability)
    {
        Destination = destination;
        Channel = channel;
        Reliability = reliability;
    }

    public void Write<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter
    {
        writer.Write(Destination);
        writer.Write(Channel);
        writer.Write((byte)Reliability);
    }

    public RelayHeader Read<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new RelayHeader(
            reader.Read<TReader, PeerId>(),
            reader.ReadUInt8(),
            (PacketReliability)reader.ReadUInt8()
        );
    }
}