using System;
using Protocol.Defines.Client;

namespace Protocol.Schemas.Client
{
    public record ClientHandshakeRequest
    {
        public ClientType Type { get; init; }

        public static byte[] AsPayload(ClientType type)
        {
            return new byte[] { (byte)type };
        }

        public static ClientHandshakeRequest FromPayload(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 1)
            {
                throw new InvalidOperationException("Invalid payload length for ClientHandshakeRequest.");
            }

            return new ClientHandshakeRequest() { Type = (ClientType)payload[0] };
        }
    }
}
