namespace HandyNetworking.Protocol;

internal enum PacketTypes
    : byte
{
    None = 0,
    Serialized = 1,
    RelayedSingle = 2,
    RelayedBroadcast = 3,
}