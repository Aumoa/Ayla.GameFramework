#nullable enable

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetEngine.Network;
using Protocol.Defines.Client;
using Protocol.Schemas.Client;
using UnityEngine;

namespace Ayla.GameFramework
{
    public class DebugGameApp : MonoBehaviour
    {
        private ulong m_TicketNo;
        private ConnectionPipeline? m_Pipeline;

        private void OnEnable()
        {
            m_Pipeline = new ConnectionPipeline(new IPEndPoint(IPAddress.Loopback, 60001), HandshakeAsync);
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            m_Pipeline.Start();
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            m_Pipeline?.Dispose();
            m_Pipeline = null;
        }

        private void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
        }

        private async Task HandshakeAsync(CancellationToken cancellationToken)
        {
            var rsp = await m_Pipeline!.SendRequestAsync(ClientMessage.HANDSHAKE, DebugGameHandshakeRequest.AsPayload(ClientType.Game), cancellationToken);
            bool failure = false;
            if (rsp.MessageId != ClientMessage.HANDSHAKE)
            {
                Debug.LogErrorFormat("Handshake failed, unexpected response message ID: {0}", rsp.MessageId);
                failure = true;
            }
            else if (DebugGameHandshakeResponse.FromPayload(rsp.Payload).Ok == false)
            {
                Debug.LogError("Handshake failed.");
                failure = true;
            }

            if (failure)
            {
                throw new InvalidOperationException("Handshake failed");
            }

            Debug.Log("The server has responded to the CLIENT_HANDSHAKE request.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StaticAwake()
        {
            var gameObject = new GameObject("DebugGameApp", typeof(DebugGameApp));
            DontDestroyOnLoad(gameObject);
        }
    }
}
