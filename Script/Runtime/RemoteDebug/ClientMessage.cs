#nullable enable

namespace Ayla.GameFramework
{
    public static class ClientMessage
    {
        public const ushort HANDSHAKE = 0x0001;

        public const ushort DEVICE_LOG = 0x0010;
        public const ushort DEVICE_INFO = 0x0011;
    }
}
