using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zlib;
using System.IO;
using Trader.Common;
namespace Trader.Server.Serialization
{
    public static class ZlibHelper
    {
        public static UnmanagedMemory ZibCompress(byte[] input)
        {
            using (var ms = new TraderMemoryStream(500 * 1024))
            {
                using (var zs = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression, true))
                {
                    zs.Write(input, 0, input.Length);
                }
                return ms.Buffer;
            }
        }

        public static byte[] ZibDecompress(byte[] data)
        {
            var ms = new MemoryStream(data);
            using (var msDecompressed = new System.IO.MemoryStream())
            {
                using (var zs = new ZlibStream(msDecompressed, CompressionMode.Decompress, true))
                {
                    int readByte = ms.ReadByte();
                    while (readByte != -1)
                    {
                        zs.WriteByte((byte)readByte);
                        readByte = ms.ReadByte();
                    }
                    zs.Flush();
                }
                return msDecompressed.ToArray();

            }
        }
    }
}
