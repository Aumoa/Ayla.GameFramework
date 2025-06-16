#nullable enable

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ayla.GameFramework
{
    internal static class SocketUtility
    {
        public static async Task SendExactlyAsync(this Socket socket, ReadOnlyMemory<byte> input, CancellationToken cancellationToken)
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

        public static async Task ReceiveExactlyAsync(this Socket socket, Memory<byte> output, CancellationToken cancellationToken)
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
    }
}
