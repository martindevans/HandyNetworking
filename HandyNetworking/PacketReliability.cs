﻿namespace HandyNetworking;

public enum PacketReliability
    : byte
{
    /// <summary>
    /// Reliable. Packets won't be dropped, won't be duplicated, can arrive without order.
    /// </summary>
    ReliableUnordered,

    /// <summary>
    /// Unreliable. Packets can be dropped, won't be duplicated, will arrive in order.
    /// </summary>
    UnreliableSequenced,

    /// <summary>
    /// Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive in order.
    /// </summary>
    ReliableOrdered,

    /// <summary>
    /// Reliable only last packet. Packets can be dropped (except the last one), won't be duplicated, will arrive in order.
    /// Cannot be fragmented
    /// </summary>
    ReliableSequenced,

    /// <summary>
    /// Unreliable. Packets can be dropped, can be duplicated, can arrive without order.
    /// </summary>
    Unreliable,
}