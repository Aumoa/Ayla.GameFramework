#nullable enable

using System.IO;
using UnityEngine;

namespace Ayla.GameFramework
{
    public record DeviceLogEntry : IDebugMessage
    {
        public string Message { get; init; } = string.Empty;

        public string StackTrace { get; init; } = string.Empty;

        public LogType LogType { get; init; }

        public Message AsMessage(ulong ticketNo)
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(Message);
            writer.Write(StackTrace);
            writer.Write((byte)LogType);
            return GameFramework.Message.Construct(ClientMessage.DEVICE_LOG, ticketNo, stream.ToArray());
        }
    }
}
