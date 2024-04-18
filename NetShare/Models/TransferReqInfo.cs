using System.Text.Json;

namespace NetShare.Models
{
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
