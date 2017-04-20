using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace XPloit.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Read lines from file (Allow gzip)
        /// </summary>
        /// <param name="path">Path</param>
        public static IEnumerable<string> ReadWordFile(string path)
        {
            Stream stream = File.OpenRead(path);

            if (DetectFileFormat(stream, true, true) == EFileFormat.Gzip)
            {
                //WriteInfo("Decompress gzip wordlist");
                //WriteInfo("Compressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);

                stream.Close();
                stream.Dispose();

                using (FileStream streamR = File.OpenRead(path))
                using (GZipStream gz = new GZipStream(streamR, CompressionMode.Decompress))
                {
                    // decompress
                    using (StreamReader sr = new StreamReader(gz, true))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null) yield return line;
                    }
                }

                //WriteInfo("Decompressed size", StringHelper.Convert2KbWithBytes(stream.Length), ConsoleColor.Green);
            }
            else using (StreamReader sr = new StreamReader(stream, true))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null) yield return line;
                }
        }
        /// <summary>
        /// Formatos de archivo reconocidos
        /// </summary>
        public enum EFileFormat
        {
            None,
            // Imagen
            Png, Jpg, Gif, Bmp, Tiff,
            // Comprimidos
            Gzip, SevenZip, Rar, PkZip,
            // Documentos
            Pdf, Office97_2003, Rtf,
            // Video
            Mp4
        }

        static byte[] HEADER_GIF = new byte[] { (byte)'G', (byte)'I', (byte)'F' };
        static byte[] HEADER_PNG = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        static byte[] HEADER_JPG = new byte[] { 0xFF, 0xD8 };
        static byte[] HEADER_GZIP = new byte[] { 0x1F, 0x8B, 0x08 };
        static byte[] HEADER_7Z = new byte[] { (byte)'7', (byte)'z', 0xBC, 0xAF, 0x27, 0x1C };
        static byte[] HEADER_TAR = new byte[] { (byte)'u', (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'\0', 0x0, 0x0 };
        static byte[] HEADER_PDF = new byte[] { (byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-' };
        static byte[] HEADER_PKZIP = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        static byte[] HEADER_BMP = new byte[] { 0x42, 0x4D };
        static byte[] HEADER_RAR = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 };
        static byte[] HEADER_OFFICE_97_2003 = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00 };
        static byte[] HEADER_RTF = new byte[] { (byte)'{', (byte)'\\', (byte)'r', (byte)'t', (byte)'f' };
        static byte[] HEADER_MP4 = new byte[] { 0, 0, 0, 24, (byte)'f', (byte)'t', (byte)'y', (byte)'p' };
        /// <summary>
        /// Detecta el tipo de archivo en base a su cabecera
        /// </summary>
        /// <param name="data">Datos</param>
        /// <returns>Devuelve el tipo de archivo</returns>
        public static EFileFormat DetectFileFormat(byte[] data)
        {
            if (CheckHeader(data, 0, HEADER_PNG)) return EFileFormat.Png;
            if (CheckHeader(data, 0, HEADER_JPG)) return EFileFormat.Jpg;
            if (CheckHeader(data, 0, HEADER_GZIP)) return EFileFormat.Gzip;
            if (CheckHeader(data, 0, HEADER_7Z)) return EFileFormat.SevenZip;
            if (CheckHeader(data, 0, HEADER_GIF)) return EFileFormat.Gif;
            if (CheckHeader(data, 0, HEADER_PDF)) return EFileFormat.Pdf;
            if (CheckHeader(data, 0, HEADER_BMP)) return EFileFormat.Bmp;
            if (CheckHeader(data, 0, HEADER_RAR)) return EFileFormat.Rar;
            if (CheckHeader(data, 0, HEADER_RTF)) return EFileFormat.Rtf;
            if (CheckHeader(data, 0, HEADER_MP4)) return EFileFormat.Mp4;
            if (CheckHeader(data, 0, HEADER_OFFICE_97_2003))
            {
                // Xls ppt doc
                return EFileFormat.Office97_2003;
            }
            if (IsTiff(data)) return EFileFormat.Tiff;

            if (CheckHeader(data, 0, HEADER_PKZIP))
            {
                // Docx Xlsx Xlsb Zip XPS
                return EFileFormat.PkZip;
            }

            return EFileFormat.None;
        }
        /// <summary>
        /// Detecta el tipo de archivo en base a su cabecera (y deja el Stream en su misma posición)
        /// </summary>
        /// <param name="data">Datos</param>
        /// <param name="seekBeginBeforeCheck">True si tiene que hacer un Seek al inicio antes de leer la cabecera</param>
        /// <param name="seekToSamePosition">True si se desea devolver a la posición en la que estaba</param>
        /// <returns>Devuelve el tipo de archivo</returns>
        public static EFileFormat DetectFileFormat(Stream data, bool seekBeginBeforeCheck, bool seekToSamePosition)
        {
            long pos = data.Position;
            if (seekBeginBeforeCheck) data.Seek(0, SeekOrigin.Begin);

            byte[] head = new byte[10];
            data.Read(head, 0, head.Length);

            if (seekToSamePosition) data.Seek(pos, SeekOrigin.Begin);

            return DetectFileFormat(head);
        }
        /// <summary>
        /// Devuelve true si la cabecera corresponde a un TIFF
        /// </summary>
        /// <param name="header">Cabecera</param>
        /// <returns>Devuelve true si la cabecera corresponde a un TIFF</returns>
        public static bool IsTiff(byte[] header)
        {
            const int kHeaderSize = 2;
            const byte kIntelMark = 0x49;
            const byte kMotorolaMark = 0x4d;
            const ushort kTiffMagicNumber = 42;

            if (header.Length < kHeaderSize)
                return false;

            if (header[0] != header[1] || (header[0] != kIntelMark && header[0] != kMotorolaMark))
                return false;
            bool isIntel = header[0] == kIntelMark;

            ushort magicNumber = 0;
            byte b0 = header[kHeaderSize];
            byte b1 = header[kHeaderSize + 1];

            if (isIntel) magicNumber = (ushort)(((int)b1 << 8) | (int)b0);
            else magicNumber = (ushort)(((int)b0 << 8) | (int)b1);

            if (magicNumber != kTiffMagicNumber) return false;
            return true;
        }
        /// <summary>
        /// Comprueba si un Array de bytes empieza por una cabecera
        /// </summary>
        /// <param name="data">Datos a comprobar</param>
        /// <param name="index">Indice</param>
        /// <param name="header">Cabecera</param>
        /// <returns>Devuelve true si compienza por la cabecera</returns>
        public static bool CheckHeader(byte[] data, int index, byte[] header)
        {
            if (data == null || header == null) return false;
            if (data.Length - index < header.Length) return false;

            for (int x = 0, y = index, my = data.Length, mx = header.Length; y < my && x < mx; x++, y++)
                if (data[y] != header[x]) return false;

            return true;
        }
    }
}