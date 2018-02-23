using System;
using System.IO;
using System.IO.Compression;

namespace XPloit.Helpers
{
    public class CompressHelper
    {
        const int BufferSize = 10240;
        /// <summary>
        /// Comprueba si está comprimido
        /// </summary>
        /// <param name="data">Data</param>
        public static bool IsGzipped(byte[] data, int index)
        {
            if (data.Length - index < 2) return false;
            return data[index] == 0x1f && data[index + 1] == 0x8b;
        }
        /// <summary>
        /// Comprime o descomprime en GZIP un buffer
        /// </summary>
        /// <param name="buff">Buffer</param>
        /// <param name="index">Posición inicial</param>
        /// <param name="count">Cantidad</param>
        /// <param name="compress">True para Comprimir, false para Descomprimir</param>
        /// <returns>Devuelve el array de bytes comprimido o descomprimido según se le pida</returns>
        public static byte[] Compress(byte[] buff, int index, int count, bool compress)
        {
            if (buff == null || buff.Length == 0) return new byte[] { };

            using (MemoryStream ms = new MemoryStream(buff, index, count))
                return Compress(ms, compress);
        }
        /// <summary>
        /// Get Ungzipped stream from file
        /// </summary>
        /// <param name="file">File</param>
        public static Stream UnGzFile(string file)
        {
            if (!File.Exists(file)) return null;

            FileStream fs = File.OpenRead(file);
            byte[] head = new byte[2];
            if (fs.Read(head, 0, 2) == 2)
            {
                if (IsGzipped(head, 0))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    return new GZipStream(fs, CompressionMode.Decompress, false);
                }
            }

            fs.Seek(0, SeekOrigin.Begin);
            return fs;
        }
        /// <summary>
        /// Comprime o descomprime en GZIP un Stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="compress">True para Comprimir, false para Descomprimir</param>
        /// <returns>Devuelve el array de bytes comprimido o descomprimido según se le pida</returns>
        public static byte[] Compress(Stream stream, bool compress)
        {
            if (stream == null || stream.Length == 0) return new byte[] { };
            byte[] buff = null;
            using (MemoryStream ms = new MemoryStream())
            {
                if (compress)
                {
                    using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, false))
                    {
                        stream.CopyTo(gzip, BufferSize);
                        gzip.Close();
                    }
                }
                else
                {
                    // Ver dispose del stream .. no tiene que hacerlo
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress, false))
                    {
                        gzip.CopyTo(ms, BufferSize);
                        gzip.Close();
                    }
                }
                buff = ms.ToArray();
                ms.Close();
            }
            return buff;
        }
    }
}