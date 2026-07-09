using BLE.Messages.Parsers;

namespace BLE.Messages.Tests;

public class DeserializerTests
{
    private readonly Deserializer _deserializer = new();
    private readonly Serializer _serializer = new();

    [Theory]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public void DataMessage_RoundTrip_PreservesAllFields(byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var data = SerializationTestHelpers.CreateDataPayload(bleVersion);
        var original = DataMessage.Create(id, data, bleVersion);

        var frame = _serializer.Serialize(original, bleVersion);
        var restored = _deserializer.GetMessage<DataMessage>(frame);

        Assert.NotNull(restored);
        Assert.Equal(MessageType.Data, restored.Type);
        Assert.Equal(bleVersion, restored.BleVersion);
        Assert.Equal(id, restored.Id);
        Assert.Equal(data, restored.Data);
    }

    [Theory]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public void ReceiptMessage_RoundTrip_PreservesAllFields(byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var original = ReceiptMessage.Create(id, bleVersion);

        var frame = _serializer.Serialize(original, bleVersion);
        var restored = _deserializer.GetMessage<ReceiptMessage>(frame);

        Assert.NotNull(restored);
        Assert.Equal(MessageType.Receipt, restored.Type);
        Assert.Equal(bleVersion, restored.BleVersion);
        Assert.Equal(id, restored.Id);
        Assert.Empty(restored.Data);
    }

    [Theory]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public void ProtocolsApprovalMessage_RoundTrip_PreservesAllFields(byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var original = ProtocolsApprovalMessage.Create(id, bleVersion);

        var frame = _serializer.Serialize(original, bleVersion);
        var restored = _deserializer.GetMessage<ProtocolsApprovalMessage>(frame);

        Assert.NotNull(restored);
        Assert.Equal(MessageType.ProtocolsApproval, restored.Type);
        Assert.Equal(bleVersion, restored.BleVersion);
        Assert.Equal(id, restored.Id);
        Assert.Empty(restored.Data);
    }

    [Theory]
    [InlineData(MessageType.Data, (byte)4)]
    [InlineData(MessageType.Receipt, (byte)4)]
    [InlineData(MessageType.ProtocolsApproval, (byte)4)]
    public void GetType_ReturnsMessageTypeFromFrame(MessageType type, byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var data = type == MessageType.Data ? SerializationTestHelpers.CreateDataPayload(bleVersion) : [];
        var frame = _serializer.Serialize(type, bleVersion, id, data);

        Assert.Equal(type, _deserializer.GetType(frame));
    }

    [Theory]
    [InlineData((byte)4)]
    [InlineData((byte)5)]
    public void BleVersion_ReturnsVersionFromFrame(byte bleVersion)
    {
        var id = SerializationTestHelpers.CreateId(bleVersion);
        var data = SerializationTestHelpers.CreateDataPayload(bleVersion);
        var frame = _serializer.Serialize(MessageType.Data, bleVersion, id, data);

        Assert.Equal(bleVersion, _deserializer.BleVersion(frame));
    }

    [Fact]
    public void GetMessage_ThrowsForUnsupportedMessageType()
    {
        var frame = new byte[] { 0xFF, 4, 1, 2, 3, 4 };

        Assert.Throws<ArgumentOutOfRangeException>(() => _deserializer.GetMessage<DataMessage>(frame));
    }

    [Fact]
    public void GetMessage_ThrowsWhenBleVersionCannotBeInferred()
    {
        var frame = new byte[] { (byte)MessageType.Data, 1, 2, 3, 4, 5 };

        Assert.Throws<ArgumentOutOfRangeException>(() => _deserializer.GetMessage<DataMessage>(frame));
    }

    [Fact]
    public void DataMessage_BLE4_IdIsFourBytes()
    {
        var id = SerializationTestHelpers.CreateId(4);
        var data = SerializationTestHelpers.CreateDataPayload(4);
        var frame = _serializer.Serialize(MessageType.Data, 4, id, data);

        var restored = _deserializer.GetMessage<DataMessage>(frame);

        Assert.NotNull(restored);
        Assert.Equal(4, restored.Id.Length);
        Assert.Equal(id, restored.Id);
    }
}