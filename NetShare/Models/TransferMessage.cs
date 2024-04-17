using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace NetShare.Models
{
    public readonly struct TransferMessage(TransferMessage.Type type, string? path = null, long dataSize = 0)
    {
        public readonly Type type = type;
        public readonly string? path = path;
        public readonly long dataSize = dataSize;

        public enum Type : byte
        {
            None,
            RequestTransfer,
            AcceptReceive,
            DeclineReceive,
            Continue,
            Cancel,
            Complete,
            File
        }
    }

    public record struct TransferReqInfo(string Sender, int TotalFiles, long TotalSize)
    {
        public static string Serialize(TransferReqInfo info)
        {
            return JsonSerializer.Serialize(info);
        }

        public static TransferReqInfo? Deserialize(string? text)
        {
            try
            {
                return JsonSerializer.Deserialize<TransferReqInfo>(text ?? "");
            }
            catch { }
            return null;
        }
    }
}
