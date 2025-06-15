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
        private byte[] m_Payload;

        private Message(ushort messageId, ulong ticketNo, byte[] payload)
        {
            MessageId = messageId;
            TicketNo = ticketNo;
            m_Payload = payload;
        }

        public BinaryReader GetPayloadReader()
        {
            return new BinaryReader(new MemoryStream(m_Payload));
        }

        public async Task SendAsync(Socket socket, CancellationToken cancellationToken)
        {
            var header = new byte[2 + 8 + 4];
            BitConverter.TryWriteBytes(header.AsSpan(0, 2), MessageId);
            BitConverter.TryWriteBytes(header.AsSpan(2, 8), TicketNo);
            BitConverter.TryWriteBytes(header.AsSpan(10, 4), (uint)m_Payload.Length);
            await SendExactlyAsync(socket, header, cancellationToken);
            await SendExactlyAsync(socket, m_Payload, cancellationToken);
        }

        public static async Task<Message> ReceiveAsync(Socket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[2 + 8 + 4];

            await ReceiveExactlyAsync(socket, buffer.AsMemory(0, 2 + 8 + 4), cancellationToken);
            ushort messageId = BitConverter.ToUInt16(buffer.AsSpan(0, 2));
            ulong ticketNo = BitConverter.ToUInt64(buffer.AsSpan(2, 8));
            uint payloadSize = BitConverter.ToUInt32(buffer.AsSpan(10, 4));

            buffer = new byte[payloadSize];
            await ReceiveExactlyAsync(socket, buffer, cancellationToken);

            return new Message(messageId, ticketNo, buffer);
        }

        public static Message Construct(ushort messageId, ulong ticketNo)
            => Construct(messageId, ticketNo, ReadOnlyMemory<byte>.Empty);

        public static Message Construct(ushort messageId, ulong ticketNo, ReadOnlyMemory<byte> payload)
        {
            return new Message(messageId, ticketNo, payload.ToArray());
        }

        private static async Task ReceiveExactlyAsync(Socket socket, Memory<byte> output, CancellationToken cancellationToken)
        {
            int recv = 0;
            while (recv < output.Length)
            {
                int bytesRead = await socket.ReceiveAsync(output[recv..], SocketFlags.None, cancellationToken);
                if (bytesRead == 0)
                {
                    throw new SocketException((int)SocketError.ConnectionReset);
                }
                recv += bytesRead;
            }
        }

        private static async Task SendExactlyAsync(Socket socket, ReadOnlyMemory<byte> input, CancellationToken cancellationToken)
        {
            int sent = 0;
            while (sent < input.Length)
            {
                int bytesSent = await socket.SendAsync(input[sent..], SocketFlags.None, cancellationToken);
                if (bytesSent == 0)
                {
                    throw new SocketException((int)SocketError.ConnectionReset);
                }
                sent += bytesSent;
            }
        }
    }
}
