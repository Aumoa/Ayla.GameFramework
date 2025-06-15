#nullable enable

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla.GameFramework
{
    public class DebugGameApp : MonoBehaviour
    {
        private void OnEnable()
        {
            _ = StartConnector(destroyCancellationToken);
        }

        private async Task StartConnector(CancellationToken cancellationToken)
        {
            Socket? socket = null;
            await Task.Yield();

            while (cancellationToken.IsCancellationRequested == false)
            {
                if (socket == null)
                {
                    try
                    {
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        await socket.ConnectAsync(IPAddress.Loopback, 60001);
                        cancellationToken.ThrowIfCancellationRequested();

                        var message = Message.Construct(ClientMessage.HANDSHAKE, 1);
                        await message.SendAsync(socket, cancellationToken);

                        var rsp = await Message.ReceiveAsync(socket, cancellationToken);
                        if (rsp.MessageId != ClientMessage.HANDSHAKE)
                        {
                            Debug.LogErrorFormat("Handshake failed, expected {0}, got {1}", ClientMessage.HANDSHAKE, rsp.MessageId);
                            socket.Dispose();
                            socket = null;
                            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                            continue;
                        }

                        Debug.LogFormat("Connected to debug server on port {0}", socket.RemoteEndPoint);
                    }
                    catch (SocketException)
                    {
                        Debug.LogErrorFormat("Failed to connect to the debug server. Retrying in 10 seconds.");
                        socket?.Dispose();
                        socket = null;
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }
                }
                else
                {
                    await Task.Yield();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StaticAwake()
        {
            var gameObject = new GameObject("DebugGameApp", typeof(DebugGameApp));
            DontDestroyOnLoad(gameObject);
        }
    }
}
