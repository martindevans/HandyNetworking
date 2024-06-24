using HandyNetworking.Serialization;
using HandySerialization;
using HandySerialization.Extensions;

namespace HandyNetworking.Protocol;

internal static class ByteWriterExtensions
{
    public static void WritePacketHeader<TWriter>(this ref TWriter writer, PacketTypes type, PeerId sender)
        where TWriter : struct, IByteWriter
    {
        writer.Write(Constants.MAGIC);
        writer.Write((byte)type);
        writer.Write(sender);
    }

    public static bool ReadPacketHeader<TReader>(this ref TReader reader, out PacketTypes type, out PeerId sender)
        where TReader : struct, IByteReader
    {
        if (reader.ReadUInt16() != Constants.MAGIC)
        {
            type = PacketTypes.None;
            sender = default;
            return false;
        }

        type = (PacketTypes)reader.ReadUInt8();
        sender = reader.Read<TReader, PeerId>();
        return true;
    }

    public static void WritePacketSerialized<TWriter, TPayload>(this ref TWriter writer, PeerId sender, TPayload payload)
        where TWriter : struct, IByteWriter
        where TPayload : IByteSerializable<TPayload>
    {
        writer.WritePacketHeader(PacketTypes.Serialized, sender);
        writer.Write(UniversalTypeId<TPayload>.Id);
        writer.Write(payload);
    }

    public static void WritePacketRelayHeader<TWriter>(this ref TWriter writer, PeerId sender, RelayHeader relayInfo)
        where TWriter : struct, IByteWriter
    {
        writer.WritePacketHeader(PacketTypes.RelayedSingle, sender);
        writer.Write(relayInfo);
    }
}