using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Models
{
    public class TransferProtocol : IDisposable
    {
        private const int bufferSize = 1024 * 1024;

        private readonly Stream stream;
        private byte[] dataWriteBuffer = new byte[bufferSize];
        private byte[] dataReadBuffer = new byte[bufferSize];

        private readonly Timer rateTimer;
        private long transferFrame;
        private long transferRate;
        private long receiveFrame;
        private long receiveRate;

        public long TransferRate => transferRate;
        public long ReceiveRate => receiveRate;

        public TransferProtocol(Stream stream)
        {
            this.stream = stream;
            rateTimer = new Timer(UpdateTransferRates, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
        }

        private void UpdateTransferRates(object? state)
        {
            long bytes = Interlocked.Exchange(ref transferFrame, 0);
            Interlocked.Exchange(ref transferRate, bytes);

            bytes = Interlocked.Exchange(ref receiveFrame, 0);
            Interlocked.Exchange(ref receiveRate, bytes);
        }

        public async Task SendAsync(TransferMessage msg, FileInfo? file = null, IProgress<long>? progress = null, CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;
            int msgSize = sizeof(int) + sizeof(byte) + sizeof(long) + (sizeof(char) * (msg.path?.Length ?? 0));
            TransferBinary.WriteInt(stream, msgSize);
            TransferBinary.WriteByte(stream, (byte)msg.type);
            TransferBinary.WriteLong(stream, file?.Length ?? msg.dataSize);
            TransferBinary.WriteString(stream, msg.path ?? "");
            stream.Flush();
            Interlocked.Add(ref transferFrame, msgSize);

            long totalRead = msgSize;
            byte[] buffer = dataWriteBuffer;
            if(file != null)
            {
                using(FileStream fs = file.OpenRead())
                {
                    int read;
                    while((read = await fs.ReadAsync(buffer, 0, buffer.Length, ct.Value)) != 0)
                    {
                        await stream.WriteAsync(buffer, 0, read, ct.Value);
                        Interlocked.Add(ref transferFrame, read);
                        progress?.Report(totalRead += read);
                    }
                }
            }
            stream.Flush();
        }

        public async Task<TransferMessage> ReadAsync(CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;

            Memory<byte> buffer = new byte[sizeof(int)];
            if(await ReadBuffer(buffer, ct.Value) != buffer.Length)
            {
                return default;
            }

            int size = TransferBinary.ReadInt(buffer.Span) - sizeof(int);
            Array.Resize(ref dataReadBuffer, Math.Max(dataReadBuffer.Length, size));
            buffer = new Memory<byte>(dataReadBuffer, 0, size);
            if(await ReadBuffer(buffer, ct.Value) != buffer.Length)
            {
                return default;
            }

            TransferMessage.Type type = (TransferMessage.Type)TransferBinary.ReadByte(buffer[..sizeof(byte)].Span);
            long dataSize = TransferBinary.ReadLong(buffer.Slice(sizeof(byte), sizeof(long)).Span);
            string path = TransferBinary.ReadString(buffer[(sizeof(byte) + sizeof(long))..].Span);
            Interlocked.Add(ref receiveFrame, size + sizeof(int));
            return new TransferMessage(type, path, dataSize);
        }

        private async Task<int> ReadBuffer(Memory<byte> buffer, CancellationToken ct)
        {
            int read;
            int totalRead = 0;
            do
            {
                read = await stream.ReadAsync(buffer[totalRead..], ct);
                totalRead += read;
            }
            while(read != 0 && totalRead < buffer.Length);
            return totalRead;
        }

        public async Task<long> ReadData(string destPath, TransferMessage msg, IProgress<long>? progress = null, CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;

            using(FileStream fs = new FileStream(destPath, FileMode.Create))
            {
                byte[] buffer = dataReadBuffer;
                long dataSize = msg.dataSize;
                while(dataSize > 0)
                {
                    int read = await stream.ReadAsync(buffer, 0, Math.Min((int)Math.Min(dataSize, int.MaxValue), buffer.Length), ct.Value);
                    dataSize -= read;
                    await fs.WriteAsync(buffer, 0, read);
                    Interlocked.Add(ref receiveFrame, read);
                    progress?.Report(msg.dataSize - dataSize);
                    if(read == 0)
                    {
                        break;
                    }
                }
                return msg.dataSize - dataSize;
            }
        }

        public void Dispose()
        {
            rateTimer?.Dispose();
        }
    }
}
