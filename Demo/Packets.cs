using HandySerialization;
using HandySerialization.Extensions;

namespace Demo;

public readonly struct KeyCharPacket
    : IByteSerializable<KeyCharPacket>
{
    public char Character { get; }

    public KeyCharPacket(char Character)
    {
        this.Character = Character;
    }

    public void Write<TWriter>(ref TWriter writer)
        where TWriter : struct, IByteWriter
    {
        writer.Write(Character);
    }

    public KeyCharPacket Read<TReader>(ref TReader reader)
        where TReader : struct, IByteReader
    {
        return new KeyCharPacket(reader.ReadChar());
    }
}