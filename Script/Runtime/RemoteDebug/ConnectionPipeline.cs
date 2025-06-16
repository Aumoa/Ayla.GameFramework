#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ayla.GameFramework;
using UnityEngine;

namespace NetEngine.Network
{
    public class ConnectionPipeline : IDisposable
    {
        private class QueuedMessage
        {
            public readonly ulong TicketNo;
            public readonly Message MessageToSend;

            private TaskCompletionSource<Message> m_TaskCompletionSource = new();

            public QueuedMessage(ulong ticketNo, Message messageToSend)
            {
                TicketNo = ticketNo;
                MessageToSend = messageToSend;
            }

            public Task<Message> ResponseTask => m_TaskCompletionSource.Task;

            public void SetResult(Message message)
            {
                m_TaskCompletionSource.TrySetResult(message);
            }

            public void SetCanceled()
            {
                m_TaskCompletionSource.TrySetCanceled();
            }
        }

        private readonly IPEndPoint m_EndPoint;
        private readonly Func<CancellationToken, Task> m_Handshake;
        private readonly List<QueuedMessage> m_SendQueue = new();
        private readonly List<QueuedMessage> m_ReceiveQueue = new();
        private readonly List<Message> m_NotifyQueue = new();

        private CancellationTokenSource? m_CancellationTokenSource;
        private Task? m_RunnerTask;
        private long m_TicketNo = 0;

        public ConnectionPipeline(IPEndPoint endPoint, Func<CancellationToken, Task> handshake)
        {
            m_EndPoint = endPoint;
            m_Handshake = handshake;
        }

        public void Dispose()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            m_CancellationTokenSource = new CancellationTokenSource();
            m_RunnerTask = Runner(m_CancellationTokenSource.Token);
        }

        public async Task<Message> SendRequestAsync(ushort messageId, byte[] payload, CancellationToken cancellationToken)
        {
            var message = Message.Construct(messageId, (ulong)Interlocked.Increment(ref m_TicketNo), payload);
            var queued = new QueuedMessage(message.TicketNo, message);

            lock (m_SendQueue)
            {
                m_SendQueue.Add(queued);
                Monitor.Pulse(m_SendQueue);
            }

            using var register = cancellationToken.Register(() =>
            {
                lock (m_SendQueue)
                {
                    // Try remove
                    m_SendQueue.Remove(queued);
                    queued.SetCanceled();
                }
            });

            return await queued.ResponseTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (m_CancellationTokenSource != null && m_RunnerTask != null)
            {
                m_CancellationTokenSource.Cancel();
                await m_RunnerTask;
            }
        }

        private async Task Runner(CancellationToken cancellationToken)
        {
            await Task.Yield();

            try
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    CancellationTokenSource scopedCancellationToken = new();

                    try
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        await socket.ConnectAsync(m_EndPoint.Address, m_EndPoint.Port);
                        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(scopedCancellationToken.Token, cancellationToken).Token;
                        var loopTask = Task.WhenAny(
                            StartSender(socket, linkedCancellationToken),
                            StartReceiver(socket, linkedCancellationToken)
                        );

                        await m_Handshake(cancellationToken);
                        var choice = await loopTask;

                        if (choice.IsFaulted)
                        {
                            Debug.LogWarning("A Connection Reset state has been detected on the socket.");
                        }
                    }
                    catch (SocketException)
                    {
                        Debug.LogWarning("Failed to connect to the Core service. Retrying in 10 seconds.");
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }

                    scopedCancellationToken.Cancel();
                }
            }
            catch (Exception)
            {
                Debug.LogError("An unrecoverable exception has occurred in the pipeline.");
                Dispose();
            }
        }

        private Task StartSender(Socket socket, CancellationToken cancellationToken)
        {
            TaskCompletionSource<object?> tcs = new();
            new Thread(() =>
            {
                try
                {
                    while (cancellationToken.IsCancellationRequested == false)
                    {
                        QueuedMessage[] messagesToSend;

                        lock (m_SendQueue)
                        {
                            if (m_SendQueue.Count == 0)
                            {
                                Monitor.Wait(m_SendQueue, 100);
                                continue;
                            }

                            messagesToSend = m_SendQueue.ToArray();
                            m_SendQueue.Clear();
                        }

                        lock (m_ReceiveQueue)
                        {
                            m_ReceiveQueue.AddRange(messagesToSend);
                        }

                        foreach (var item in messagesToSend)
                        {
                            item.MessageToSend.SendAsync(socket, cancellationToken).Wait();
                        }
                    }

                    tcs.SetResult(null);
                }
                catch (OperationCanceledException)
                {
                    tcs.SetCanceled();
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            }).Start();

            return tcs.Task;
        }

        private async Task StartReceiver(Socket socket, CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                var message = await Message.ReceiveAsync(socket, cancellationToken);
                QueuedMessage? queuedMessage;

                lock (m_ReceiveQueue)
                {
                    var index = m_ReceiveQueue.FindIndex(p => p.TicketNo == message.TicketNo);
                    if (index != -1)
                    {
                        queuedMessage = m_ReceiveQueue[index];
                        m_ReceiveQueue.RemoveAt(index);
                    }
                    else
                    {
                        queuedMessage = null;
                    }
                }

                if (queuedMessage == null)
                {
                    lock (m_NotifyQueue)
                    {
                        m_NotifyQueue.Add(message);
                    }
                }
                else
                {
                    _ = Task.Run(() => queuedMessage.SetResult(message), CancellationToken.None);
                }
            }
        }
    }
}
