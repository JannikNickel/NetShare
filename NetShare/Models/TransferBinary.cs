using System;
using System.Buffers.Binary;
using System.IO;

namespace NetShare.Models
{
    public static class TransferBinary
    {
        public static void WriteByte(Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        public static void WriteInt(Stream stream, int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static void WriteLong(Stream stream, long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        public static unsafe void WriteString(Stream stream, string value)
        {
            int bytes = value.Length * sizeof(char);
            fixed(char* ptr = value)
            {
                byte* bytePtr = (byte*)ptr;
                for(int i = 0;i < bytes;i++)
                {
                    stream.WriteByte(bytePtr[i]);
                }
            }
        }

        public static byte ReadByte(ReadOnlySpan<byte> bytes)
        {
            return bytes[0];
        }

        public static int ReadInt(ReadOnlySpan<byte> bytes)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(bytes);
        }

        public static long ReadLong(ReadOnlySpan<byte> bytes)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(bytes);
        }

        public static unsafe string ReadString(ReadOnlySpan<byte> bytes)
        {
            int charCount = bytes.Length / sizeof(char);
            fixed(byte* ptr = bytes)
            {
                char* charPtr = (char*)ptr;
                return new string(charPtr, 0, charCount);
            }
        }
    }
}
