using System;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Helpers.Crypt;
using XPloit.Core.Sockets.Interfaces;
using XPloit.Core.Sockets.Exceptions;
using System.IO;

namespace XPloit.Core.Sockets.Protocols
{
    // 4 bytes length msg ,msg -> read first 4 bytes length
    public class XPloitSocketProtocol : IXPloitSocketProtocol
    {
        AESHelper _Crypt;
        Encoding _Codec;

        const int PartsHeaderLength = 3;

        public enum EProtocolMode : byte
        {
            /// <summary>
            /// Max 255 - 1 byte
            /// </summary>
            Byte = 1,
            /// <summary>
            /// Max 65535 - 2 bytes
            /// </summary>
            UInt16 = 2,
            /// <summary>
            /// Max 16581375 - 3 bytes
            /// </summary>
            UInt24 = 3,
        }

        EProtocolMode _Mode;
        byte _HeaderLength;
        int _MaxLength;
        byte[] _HeaderPadding;

        /// <summary>
        /// Max Length message
        /// </summary>
        public int MaxLength { get { return _MaxLength; } }
        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Codec { get { return _Codec; } }

        public XPloitSocketProtocol(Encoding codec, AESHelper crypt, EProtocolMode mode)
        {
            _Codec = codec;
            _Crypt = crypt;
            _Mode = mode;

            // Header is present
            _HeaderLength = (byte)mode;

            _HeaderPadding = new byte[_HeaderLength];
            _MaxLength = (int)Math.Pow(255, _HeaderLength);
            WriteLengthInPacket(_HeaderPadding, 0, _MaxLength);
        }
        public XPloitSocketProtocol(Encoding codec, EProtocolMode mode) : this(codec, null, mode) { }
        public XPloitSocketProtocol(AESHelper crypt, EProtocolMode mode) : this(Encoding.UTF8, crypt, mode) { }

        void ReadFull(Stream stream, byte[] data, int index, int length)
        {
            int dv = 0;

            do
            {
                dv = stream.Read(data, index, length);
                length -= dv;
                index += dv;
            }
            while (length > 0);
        }

        public virtual int Send(IXPloitSocketMsg msg, Stream stream)
        {
            if (stream == null || msg == null) return 0;

            byte[] bff;
            if (_Crypt != null)
            {
                bff = msg.Serialize(_Codec, null);
                bff = _Crypt.Encrypt(bff, _HeaderPadding);
            }
            else
            {
                bff = msg.Serialize(_Codec, _HeaderPadding);
            }

            int length = bff.Length;
            if (length == 0) return 0;

            if (length >= _MaxLength)
            {
                // Dividir en partes mas pequeñas
                int lengthH = length - _HeaderLength;
                int maxPacketLength = _MaxLength - _HeaderLength;
                uint packets = (uint)(lengthH / maxPacketLength);
                if (lengthH % maxPacketLength != 0) packets++;

                byte[] pak = BitConverterHelper.GetBytesUInt24(packets);

                int write = 0;
                int index = _HeaderLength;

                int currentLength;
                byte[] data = new byte[_MaxLength];    // Guid-length

                lock (stream)
                {
                    // Header and Parts
                    stream.Write(_HeaderPadding, 0, _HeaderLength);
                    stream.Write(pak, 0, PartsHeaderLength);

                    for (int x = 0; x < packets; x++)
                    {
                        currentLength = Math.Min(length - index, maxPacketLength);
                        WriteLengthInPacket(data, 0, currentLength);

                        Array.Copy(bff, index, data, _HeaderLength, currentLength);
                        index += currentLength;

                        stream.Write(data, 0, currentLength + _HeaderLength);
                        write += currentLength + _HeaderLength;
                    }
                }

                return write;
            }

            // Append length to header
            WriteLengthInPacket(bff, 0, length - _HeaderLength);

            //Log(bff, 0, length, true, cl.Parent.IsServer);
            stream.Write(bff, 0, length);

            return length;
        }
        public virtual IXPloitSocketMsg Read(Stream stream)
        {
            // Comprobar que la cabera es menor que el paquere
            if (stream == null) return null;

            int index = 0, msgLength;
            byte[] bxfData;

            lock (stream)
            {
                msgLength = ReadMessageLength(stream);

                // Control de tamaño máximo
                if (msgLength >= _MaxLength)
                {
                    if (msgLength == _MaxLength)
                    {
                        // Mensaje partido, parsear
                        byte[] parts = new byte[PartsHeaderLength];
                        ReadFull(stream, parts, 0, PartsHeaderLength);

                        uint iparts = BitConverterHelper.ToUInt24(parts, 0);

                        using (MemoryStream ms = new MemoryStream())
                        {
                            for (int x = 0; x < iparts; x++)
                            {
                                // Tamaño
                                msgLength = ReadMessageLength(stream);

                                bxfData = new byte[msgLength];
                                ReadFull(stream, bxfData, 0, msgLength);

                                ms.Write(bxfData, 0, msgLength);
                            }

                            bxfData = ms.ToArray();
                            msgLength = bxfData.Length;
                        }
                    }
                    else throw (new ProtocolException());
                }
                else
                {
                    bxfData = new byte[msgLength];
                    ReadFull(stream, bxfData, 0, msgLength);
                }
            }

            //Log(msg, index - 4, length + 4, false, cl.Parent.IsServer);
            if (_Crypt != null)
            {
                //desencriptamos el mensaje
                bxfData = _Crypt.Decrypt(bxfData, index, ref msgLength);
                if (bxfData == null) return null;

                index = 0;
            }

            return IXPloitSocketMsg.Deserialize(_Codec, bxfData, index, msgLength);
        }

        #region Helpers
        void WriteLengthInPacket(byte[] bff, int index, int length)
        {
            // Append length to header
            switch (_HeaderLength)
            {
                case 1:
                    {
                        bff[index] = (byte)(length);
                        break;
                    }
                case 2:
                    {
                        byte[] bft = BitConverterHelper.GetBytesUInt16((ushort)(length));
                        Array.Copy(bft, 0, bff, index, _HeaderLength);
                        break;
                    }
                case 3:
                    {
                        byte[] bft = BitConverterHelper.GetBytesUInt24((uint)(length));
                        Array.Copy(bft, 0, bff, index, _HeaderLength);
                        break;
                    }
                case 4:
                    {
                        byte[] bft = BitConverterHelper.GetBytesInt32(length);
                        Array.Copy(bft, 0, bff, index, _HeaderLength);
                        break;
                    }
            }
        }
        int ReadMessageLength(Stream stream)
        {
            // Asignamos el tamaño del mensaje
            switch (_HeaderLength)
            {
                case 1: return stream.ReadByte();
                case 2:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        return BitConverterHelper.ToUInt16(bxf, 0);
                    }
                case 3:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        return (int)BitConverterHelper.ToUInt24(bxf, 0);
                    }
                case 4:
                    {
                        byte[] bxf = new byte[_HeaderLength];
                        ReadFull(stream, bxf, 0, _HeaderLength);
                        return BitConverterHelper.ToInt32(bxf, 0);
                    }
                default: throw (new ProtocolException());
            }
        }
        #endregion
        public bool Connect(XPloitSocketClient client) { return true; }
    }
}