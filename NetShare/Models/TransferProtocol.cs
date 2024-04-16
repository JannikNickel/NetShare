using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Models
{
    public class TransferProtocol(TcpClient client)
    {
        private const int bufferSize = 1024 * 1024;

        private TcpClient client = client;
        private NetworkStream stream = client.GetStream();
        private byte[] dataReadBuffer = new byte[bufferSize];

        public async Task SendAsync(TransferMessage msg, FileInfo? file = null, CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;
            int msgSize = sizeof(int) + sizeof(byte) + sizeof(long) + (sizeof(char) * (msg.path?.Length ?? 0));
            TransferBinary.WriteInt(stream, msgSize);
            TransferBinary.WriteByte(stream, (byte)msg.type);
            TransferBinary.WriteLong(stream, file?.Length ?? 0);
            TransferBinary.WriteString(stream, msg.path ?? "");
            stream.Flush();
            if(file != null)
            {
                using(FileStream fs = file.OpenRead())
                {
                    await fs.CopyToAsync(stream, bufferSize, ct.Value);
                }
            }
        }

        public async Task<TransferMessage> ReadAsync(CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;

            Memory<byte> buffer = new byte[sizeof(int)];
            int read;
            int totalRead = 0;
            do
            {
                read = await stream.ReadAsync(buffer[totalRead..], ct.Value);
                totalRead += read;
            }
            while(read != 0 && totalRead < sizeof(int));
            if(totalRead == 0)
            {
                return default;
            }

            int size = TransferBinary.ReadInt(buffer.Span) - sizeof(int);
            Array.Resize(ref dataReadBuffer, Math.Max(dataReadBuffer.Length, size));
            buffer = dataReadBuffer;
            read = 0;
            totalRead = 0;
            do
            {
                read = await stream.ReadAsync(buffer[read..size], ct.Value);
                totalRead += read;
            }
            while(read != 0 && totalRead < size);
            if(totalRead == 0)
            {
                return default;
            }

            TransferMessage.Type type = (TransferMessage.Type)TransferBinary.ReadByte(buffer[..sizeof(byte)].Span);
            long dataSize = TransferBinary.ReadLong(buffer.Slice(sizeof(byte), sizeof(long)).Span);
            string path = TransferBinary.ReadString(buffer[(sizeof(byte) + sizeof(long))..size].Span);
            return new TransferMessage(type, path, dataSize);
        }

        public async Task ReadData(string destPath, TransferMessage msg)
        {
            using(FileStream fs = new FileStream(destPath, FileMode.Create))
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = dataReadBuffer;
                long dataSize = msg.dataSize;
                while(dataSize > 0)
                {
                    int read = await stream.ReadAsync(buffer, 0, Math.Min((int)Math.Min(dataSize, int.MaxValue), buffer.Length));
                    dataSize -= read;
                    await fs.WriteAsync(buffer, 0, read);
                    if(read == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
