using BLE.Messages.Parsers;

namespace BLE.Messages.Tests;

public class SerializerTests
{
    private readonly Serializer _serializer = new();

    [Theory]
    [InlineData(MessageType.Data, (byte)4)]
    [InlineData(MessageType.Data, (byte)5)]
    [InlineData(MessageType.Receipt, (byte)4)]
    [InlineData(MessageType.Receipt, (byte)5)]
    [InlineData(MessageType.ProtocolsApproval, (byte)4)]
    [InlineData(MessageType.ProtocolsApproval, (byte)5)]
    public void Serialize_ProducesExpectedFrameLayout(MessageType type, byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var data = type == MessageType.Data
            ? SerializationTestHelpers.CreateDataPayload(bleVersion)
            : [];

        var frame = _serializer.Serialize(type, bleVersion, id, data);

        Assert.Equal((byte)type, frame[0]);
        Assert.Equal(id, frame[1..(1 + id.Length)]);
        Assert.Equal(data, frame[(1 + id.Length)..]);

        if (bleVersion == 4)
            Assert.Equal(4, id.Length);
    }

    [Theory]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public void Serialize_MessageOverload_MatchesLowLevelSerialize(byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var data = SerializationTestHelpers.CreateDataPayload(bleVersion);
        var message = DataMessage.Create(id, data, bleVersion);

        var fromMessage = _serializer.Serialize(message, bleVersion);
        var fromParts = _serializer.Serialize(MessageType.Data, bleVersion, id, data);

        Assert.Equal(fromParts, fromMessage);
    }

    [Fact]
    public void Serialize_BLE4_ThrowsWhenIdIsNotFourBytes()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            _serializer.Serialize(MessageType.Data, 4, [0x01, 0x02, 0x03], []));

        Assert.Contains("4 bytes", exception.Message);
    }

    [Fact]
    public void Serialize_BLE5_ThrowsWhenIdIsNotThirtyTwoBytes()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            _serializer.Serialize(MessageType.Data, 5, new byte[16], []));

        Assert.Contains("32 bytes", exception.Message);
    }

    [Fact]
    public void Serialize_ThrowsForUnsupportedBleVersion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _serializer.Serialize(MessageType.Data, 3, SerializationTestHelpers.CreateId(4), []));
    }
}
