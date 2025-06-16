#nullable enable

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ayla.GameFramework
{
    public class Message
    {
        public readonly ushort MessageId;
        public readonly ulong TicketNo;
        public readonly byte[] Payload;

        private Message(ushort messageId, ulong ticketNo, byte[] payload)
        {
            MessageId = messageId;
            TicketNo = ticketNo;
            Payload = payload;
        }

        public BinaryReader GetPayloadReader()
        {
            return new BinaryReader(new MemoryStream(Payload));
        }

        public async Task SendAsync(Socket socket, CancellationToken cancellationToken)
        {
            var header = new byte[2 + 8 + 4];
            BitConverter.TryWriteBytes(header.AsSpan(0, 2), MessageId);
            BitConverter.TryWriteBytes(header.AsSpan(2, 8), TicketNo);
            BitConverter.TryWriteBytes(header.AsSpan(10, 4), (uint)Payload.Length);
            await socket.SendExactlyAsync(header, cancellationToken);
            await socket.SendExactlyAsync(Payload, cancellationToken);
        }

        public static async Task<Message> ReceiveAsync(Socket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[2 + 8 + 4];

            await socket.ReceiveExactlyAsync(buffer.AsMemory(0, 2 + 8 + 4), cancellationToken);
            ushort messageId = BitConverter.ToUInt16(buffer.AsSpan(0, 2));
            ulong ticketNo = BitConverter.ToUInt64(buffer.AsSpan(2, 8));
            uint payloadSize = BitConverter.ToUInt32(buffer.AsSpan(10, 4));

            buffer = new byte[payloadSize];
            await socket.ReceiveExactlyAsync(buffer, cancellationToken);

            return new Message(messageId, ticketNo, buffer);
        }

        public static Message Construct(ushort messageId, ulong ticketNo)
            => Construct(messageId, ticketNo, ReadOnlyMemory<byte>.Empty);

        public static Message Construct(ushort messageId, ulong ticketNo, ReadOnlyMemory<byte> payload)
        {
            return new Message(messageId, ticketNo, payload.ToArray());
        }
    }
}
