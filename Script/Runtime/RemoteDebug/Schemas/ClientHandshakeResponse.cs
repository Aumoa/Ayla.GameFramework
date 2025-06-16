using System;

namespace Protocol.Schemas.Client
{
    public record ClientHandshakeResponse
    {
        public bool Ok { get; init; }

        public static byte[] AsPayload(bool ok)
        {
            return new byte[] { ok ? (byte)1 : (byte)0 };
        }

        public static ClientHandshakeResponse FromPayload(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 1)
            {
                throw new ArgumentException("Invalid payload length for ClientHandshakeResponse.", nameof(payload));
            }

            return new ClientHandshakeResponse
            {
                Ok = payload[0] != 0
            };
        }
    }
}
