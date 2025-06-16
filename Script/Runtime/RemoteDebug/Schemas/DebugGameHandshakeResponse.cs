using System;

namespace Protocol.Schemas.Client
{
    public record DebugGameHandshakeResponse
    {
        public bool Ok { get; init; }

        public static byte[] AsPayload(bool ok)
        {
            return new byte[] { ok ? (byte)1 : (byte)0 };
        }

        public static DebugGameHandshakeResponse FromPayload(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 1)
            {
                throw new ArgumentException("Invalid payload length for ClientHandshakeResponse.", nameof(payload));
            }

            return new DebugGameHandshakeResponse
            {
                Ok = payload[0] != 0
            };
        }
    }
}
