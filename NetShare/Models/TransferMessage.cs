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
            Cancel,
            Complete,
            File,
            Directory
        }
    }
}
