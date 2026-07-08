namespace BLE.Messages.Tests;

internal static class SerializationTestHelpers
{
    public static byte[] CreateId(byte bleVersion) =>
        bleVersion switch
        {
            4 => [0x01, 0x02, 0x03, 0x04],
            5 => Enumerable.Range(1, 32).Select(i => (byte)i).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(bleVersion))
        };

    public static byte[] CreateDataPayload(byte bleVersion) =>
        bleVersion switch
        {
            4 => Enumerable.Range(0x10, 17).Select(i => (byte)i).ToArray(),
            5 => Enumerable.Range(0x20, 221).Select(i => (byte)i).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(bleVersion))
        };
}
