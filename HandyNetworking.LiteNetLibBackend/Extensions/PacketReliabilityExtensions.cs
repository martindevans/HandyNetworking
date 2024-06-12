using LiteNetLib;

namespace HandyNetworking.LiteNetLibBackend.Extensions;

internal static class PacketReliabilityExtensions
{
    public static DeliveryMethod ToDeliveryMethod(this PacketReliability mode)
    {
        return mode switch
        {
            PacketReliability.ReliableUnordered => DeliveryMethod.ReliableUnordered,
            PacketReliability.UnreliableSequenced => DeliveryMethod.Sequenced,
            PacketReliability.ReliableOrdered => DeliveryMethod.ReliableOrdered,
            PacketReliability.ReliableSequenced => DeliveryMethod.ReliableSequenced,
            PacketReliability.Unreliable => DeliveryMethod.Unreliable,

            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}