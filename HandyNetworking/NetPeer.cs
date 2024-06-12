using System.Diagnostics;
using HandySerialization;
using HandySerialization.Extensions;

namespace HandyNetworking
{
    [DebuggerDisplay("{Id} ({Latency})")]
    public class NetPeer
    {
        public PeerId Id { get; }
        public TimeSpan Latency { get; internal set; }

        public NetPeer(PeerId id)
        {
            Id = id;
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