using System.Diagnostics;
using HandySerialization;
using HandySerialization.Extensions;

namespace HandyNetworking
{
    [DebuggerDisplay("{Id} ({Latency})")]
    public class NetPeer
    {
        private readonly ISender _sender;

        public bool IsLocal { get; }
        public PeerId Id { get; }
        public TimeSpan Latency { get; internal set; }

        internal NetPeer(ISender sender, bool local, PeerId id)
        {
            _sender = sender;

            IsLocal = local;
            Id = id;
        }

        public void Send<T>(T packet, byte channel, PacketReliability reliability)
            where T : struct, IByteSerializable<T>
        {
            if (IsLocal)
                throw new InvalidOperationException("Cannot send packet to local peer");

            _sender.Send(Id, packet, channel, reliability);
        }
    }

    public readonly record struct PeerId(int Value)
        : IByteSerializable<PeerId>
    {
        public void Write<TWriter>(ref TWriter writer)
            where TWriter : struct, IByteWriter
        {
            writer.Write(Value);
        }

        public PeerId Read<TReader>(ref TReader reader)
            where TReader : struct, IByteReader
        {
            return new PeerId(reader.ReadInt32());
        }
    }
}