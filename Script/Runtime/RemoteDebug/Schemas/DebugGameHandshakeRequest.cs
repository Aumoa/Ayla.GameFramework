#nullable enable
using System;
using Protocol.Defines.Client;

namespace Protocol.Schemas.Client
{
    public record DebugGameHandshakeRequest
    {
        public ClientType Type { get; init; }

        public string DeviceMode { get; init; } = string.Empty;

        public string DeviceName { get; init; } = string.Empty;

        public string OperatingSystem { get; init; } = string.Empty;

        public static byte[] AsPayload(ClientType type)
        {
            return new byte[] { (byte)type };
        }

        public static DebugGameHandshakeRequest FromPayload(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 1)
            {
                throw new InvalidOperationException("Invalid payload length for ClientHandshakeRequest.");
            }

            return new DebugGameHandshakeRequest() { Type = (ClientType)payload[0] };
        }
    }
}
