#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla.GameFramework
{
    public class DebugGameApp : MonoBehaviour
    {
        private CancellationTokenSource? m_ConnectorCancellation;
        private readonly Dictionary<ulong, Message> m_PendingMessages = new Dictionary<ulong, Message>();
        private readonly List<IDebugMessage> m_PendingSendMessages = new List<IDebugMessage>();
        private ulong m_TicketNo;

        private void OnEnable()
        {
            m_ConnectorCancellation = new CancellationTokenSource();
            _ = StartConnector(m_ConnectorCancellation.Token);
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            m_ConnectorCancellation!.Cancel();
            m_ConnectorCancellation.Dispose();
            m_ConnectorCancellation = null;
        }

        private void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            lock (m_PendingSendMessages)
            {
                m_PendingSendMessages.Add(new DeviceLogEntry
                {
                    Message = message,
                    StackTrace = stackTrace,
                    LogType = type
                });

                Monitor.Pulse(m_PendingSendMessages);
            }
        }

        private async Task StartConnector(CancellationToken cancellationToken)
        {
            Socket? socket = null;
            await Task.Yield();

            while (cancellationToken.IsCancellationRequested == false)
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
                    m_TicketNo = 2; // Start ticket number from 2 to avoid conflict with handshake message

                    await Task.WhenAll(
                        StartReceiver(socket, cancellationToken),
                        StartSender(socket, cancellationToken)
                    );
                }
                catch (SocketException)
                {
                    Debug.LogErrorFormat("Failed to connect to the debug server. Retrying in 10 seconds.");
                    socket?.Dispose();
                    socket = null;
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }

        private async Task StartReceiver(Socket socket, CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                var receiveMessage = await Message.ReceiveAsync(socket, cancellationToken);
                lock (m_PendingMessages)
                {
                    m_PendingMessages[receiveMessage.TicketNo] = receiveMessage;
                    Monitor.Pulse(m_PendingMessages);
                }
            }
        }

        private Task StartSender(Socket socket, CancellationToken cancellationToken)
        {
            TaskCompletionSource<object?> threadCompleted = new();

            new Thread(() =>
            {
                try
                {
                    while (cancellationToken.IsCancellationRequested == false)
                    {
                        IDebugMessage? messageToSend = null;
                        lock (m_PendingSendMessages)
                        {
                            if (m_PendingSendMessages.Count > 0)
                            {
                                foreach (var pendingMessage in m_PendingSendMessages)
                                {
                                    messageToSend = pendingMessage;
                                    m_PendingSendMessages.Remove(pendingMessage);
                                    break;
                                }
                            }
                            else
                            {
                                Monitor.Wait(m_PendingSendMessages, 100);
                                continue;
                            }
                        }

                        var message = messageToSend!.AsMessage(m_TicketNo++);
                        message.SendAsync(socket!, cancellationToken).Wait();
                    }

                    threadCompleted.SetResult(null);
                }
                catch (OperationCanceledException)
                {
                    threadCompleted.SetCanceled();
                }
                catch (Exception e)
                {
                    threadCompleted.SetException(e);
                }
            }).Start();

            return threadCompleted.Task;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StaticAwake()
        {
            var gameObject = new GameObject("DebugGameApp", typeof(DebugGameApp));
            DontDestroyOnLoad(gameObject);
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
    }
}
